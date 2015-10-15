﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;
    using XTask.Systems.File;

    internal static partial class NativeMethods
    {
        internal static class VolumeManagement
        {
            // Putting private P/Invokes in a subclass to allow exact matching of signatures for perf on initial call and reduce string count
            [SuppressUnmanagedCodeSecurity] // We don't want a stack walk with every P/Invoke.
            private static class Private
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365461.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern uint QueryDosDeviceW(
                    string lpDeviceName,
                    IntPtr lpTargetPath,
                    uint ucchMax);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364975.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern uint GetLogicalDriveStringsW(
                    uint nBufferLength,
                    IntPtr lpBuffer);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364996.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool GetVolumePathNameW(
                    string lpszFileName,
                    IntPtr lpszVolumePathName,
                    uint cchBufferLength);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364998.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool GetVolumePathNamesForVolumeNameW(
                    string lpszVolumeName,
                    IntPtr lpszVolumePathNames,
                    uint cchBuferLength,
                    ref uint lpcchReturnLength);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364994.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static extern bool GetVolumeNameForVolumeMountPointW(
                    string lpszVolumeMountPoint,
                    IntPtr lpszVolumeName,
                    uint cchBufferLength);

                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa364993.aspx
                [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
                internal static extern bool GetVolumeInformationW(
                   string lpRootPathName,
                   IntPtr lpVolumeNameBuffer,
                   uint nVolumeNameSize,
                   out uint lpVolumeSerialNumber,
                   out uint lpMaximumComponentLength,
                   out FileSystemFeature lpFileSystemFlags,
                   IntPtr lpFileSystemNameBuffer,
                   uint nFileSystemNameSize);
            }

            [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
            public static IEnumerable<string> QueryDosDevice(string deviceName)
            {
                if (deviceName != null) deviceName = Paths.RemoveTrailingSeparators(deviceName);

                // Null will return everything defined- this list is quite large so set a higher initial allocation
                using (var buffer = new StringBuffer(deviceName == null ? (int)8192 : 256))
                {
                    uint result = 0;

                    // QueryDosDevicePrivate takes the buffer count in TCHARs, which is 2 bytes for Unicode (WCHAR)
                    while ((result = Private.QueryDosDeviceW(deviceName, buffer, (uint)buffer.Capacity)) == 0)
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        switch (lastError)
                        {
                            case WinError.ERROR_INSUFFICIENT_BUFFER:
                                buffer.Capacity *= 2;
                                break;
                            default:
                                throw GetIoExceptionForError(lastError, deviceName);
                        }
                    }

                    buffer.Length = (int)result;
                    return buffer.Split('\0');
                }
            }

            [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
            internal static IEnumerable<string> GetLogicalDriveStrings()
            {
                using (var buffer = new StringBuffer())
                {
                    uint result = 0;

                    // GetLogicalDriveStringsPrivate takes the buffer count in TCHARs, which is 2 bytes for Unicode (WCHAR)
                    while ((result = Private.GetLogicalDriveStringsW((uint)buffer.Capacity, buffer)) > (uint)buffer.Capacity)
                    {
                        buffer.Capacity = result;
                    }

                    if (result == 0)
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(lastError);
                    }

                    buffer.Length = (int)result;
                    return buffer.Split('\0');
                }
            }

            [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
            internal static string GetVolumePathName(string path)
            {
                // Most paths are mounted at the root, 50 should handle the canonical (guid) root
                using (var volumePathName = new StringBuffer(initialCapacity: 50))
                {
                    while (!Private.GetVolumePathNameW(path, volumePathName, (uint)volumePathName.Capacity))
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        switch (lastError)
                        {
                            case WinError.ERROR_FILENAME_EXCED_RANGE:
                                volumePathName.Capacity *= 2;
                                break;
                            default:
                                throw GetIoExceptionForError(lastError, path);
                        }
                    }

                    volumePathName.SetLengthToFirstNull();
                    return volumePathName.ToString();
                }
            }

            [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
            internal static IEnumerable<string> GetVolumePathNamesForVolumeName(string volumeName)
            {
                using (var buffer = new StringBuffer())
                {
                    uint returnLength = 0;

                    // GetLogicalDriveStringsPrivate takes the buffer count in TCHARs, which is 2 bytes for Unicode (WCHAR)
                    while (!Private.GetVolumePathNamesForVolumeNameW(volumeName, buffer, (uint)buffer.Capacity, ref returnLength))
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        switch (lastError)
                        {
                            case WinError.ERROR_MORE_DATA:
                                buffer.Capacity = returnLength;
                                break;
                            default:
                                throw GetIoExceptionForError(lastError, volumeName);
                        }
                    }

                    buffer.Length = (int)returnLength;
                    return buffer.Split('\0');
                }
            }

            internal static string GetVolumeNameForVolumeMountPoint(string volumeMountPoint)
            {
                volumeMountPoint = Paths.AddTrailingSeparator(volumeMountPoint);

                // MSDN claims 50 is "reasonable", let's go double.
                using (var volumeName = new StringBuffer(initialCapacity: 100))
                {
                    if (!Private.GetVolumeNameForVolumeMountPointW(volumeMountPoint, volumeName, (uint)volumeName.Capacity))
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(lastError, volumeMountPoint);
                    }

                    volumeName.SetLengthToFirstNull();
                    return volumeName.ToString();
                }
            }

            internal static VolumeInformation GetVolumeInformation(string rootPath)
            {
                rootPath = Paths.AddTrailingSeparator(rootPath);

                using (var volumeName = new StringBuffer(initialCapacity: Paths.MaxPath + 1))
                using (var fileSystemName = new StringBuffer(initialCapacity: Paths.MaxPath + 1))
                {
                    uint serialNumber, maxComponentLength;
                    FileSystemFeature flags;
                    if (!Private.GetVolumeInformationW(rootPath, volumeName, (uint)volumeName.Capacity, out serialNumber, out maxComponentLength, out flags, fileSystemName, (uint)fileSystemName.Capacity))
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        throw GetIoExceptionForError(lastError, rootPath);
                    }

                    volumeName.SetLengthToFirstNull();
                    fileSystemName.SetLengthToFirstNull();

                    VolumeInformation info = new VolumeInformation
                    {
                        RootPathName = rootPath,
                        VolumeName = volumeName.ToString(),
                        VolumeSerialNumber = serialNumber,
                        MaximumComponentLength = maxComponentLength,
                        FileSystemFlags = flags,
                        FileSystemName = fileSystemName.ToString()
                    };

                    return info;
                }
            }
        }
    }
}

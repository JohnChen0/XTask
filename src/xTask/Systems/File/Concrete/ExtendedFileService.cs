﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File.Concrete
{
    using Interop;
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;

    /// <summary>
    /// Basic implementation of extended file service support. These methods have no .NET implementation.
    /// </summary>
    public class ExtendedFileService : IExtendedFileService
    {
        public string GetFinalPath(string path, bool resolveLinks = false)
        {
            return NativeMethods.FileManagement.GetFinalPathName(path, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED, resolveLinks);
        }

        public string GetLongPath(string path)
        {
            return NativeMethods.FileManagement.GetLongPathName(path);
        }

        public string GetShortPath(string path)
        {
            return NativeMethods.FileManagement.GetShortPathName(path);
        }

        public string GetVolumeName(string volumeMountPoint)
        {
            if (string.IsNullOrWhiteSpace(volumeMountPoint)) throw new ArgumentNullException(nameof(volumeMountPoint));

            return NativeMethods.VolumeManagement.GetVolumeNameForVolumeMountPoint(volumeMountPoint);
        }

        public string GetMountPoint(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            return NativeMethods.VolumeManagement.GetVolumePathName(path);
        }

        public IEnumerable<string> GetVolumeMountPoints(string volumeName)
        {
            if (string.IsNullOrWhiteSpace(volumeName)) throw new ArgumentNullException(nameof(volumeName));

            return NativeMethods.VolumeManagement.GetVolumePathNamesForVolumeName(volumeName);
        }

        public IEnumerable<string> QueryDosDeviceNames(string dosAlias)
        {
            return NativeMethods.VolumeManagement.QueryDosDevice(dosAlias);
        }

        public IEnumerable<string> GetLogicalDriveStrings()
        {
            return NativeMethods.VolumeManagement.GetLogicalDriveStrings();
        }

        public VolumeInformation GetVolumeInformation(string rootPath)
        {
            return NativeMethods.VolumeManagement.GetVolumeInformation(rootPath);
        }

        public IEnumerable<AlternateStreamInformation> GetAlternateStreamInformation(string path)
        {
            return NativeMethods.Backup.GetAlternateStreamInformation(path);
        }

        public bool CanCreateSymbolicLinks()
        {
            // Assuming that the current thread can replicate rights from the process
            using (var processToken = NativeMethods.Authorization.OpenProcessToken(TokenAccessLevels.Query | TokenAccessLevels.Read))
            {
                return NativeMethods.Authorization.HasPrivilege(processToken, Privileges.SeCreateSymbolicLinkPrivilege);
            }
        }
    }
}

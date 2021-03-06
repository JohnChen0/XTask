﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Interop
{
    using FileSystem;
    using FluentAssertions;
    using System;
    using System.IO;
    using Systems.File.Concrete;
    using XTask.Interop;
    using XTask.Systems.File;
    using Xunit;

    public class FileManagementTests
    {
        [Theory
            InlineData("")]
        public void FullPathErrorCases(string value)
        {
            Action action = () => NativeMethods.FileManagement.GetFullPathName(value);
            action.ShouldThrow<IOException>();
        }

        [Theory
            Trait("Environment", "CurrentDirectory")
            InlineData(@"C:", @"C:\Users")
            InlineData(@"C", @"D:\Temp\C")
            ]
        public void ValidateKnownRelativeBehaviors(string value, string expected)
        {
            // Set the current directory to D: and the hidden env for C:'s last current directory
            NativeMethods.SetEnvironmentVariable(@"=C:", @"C:\Users");
            using (new TempCurrentDirectory(@"D:\Temp"))
            {
                NativeMethods.FileManagement.GetFullPathName(value).Should().Be(expected);
            }
        }

        private class TempCurrentDirectory : IDisposable
        {
            private string priorDirectory;

            public TempCurrentDirectory(string directory)
            {
                priorDirectory = Environment.CurrentDirectory;
                Environment.CurrentDirectory = directory;
            }

            public void Dispose()
            {
                Environment.CurrentDirectory = priorDirectory;
            }
        }

        [Theory
            // Basic dot space handling
            InlineData(@"C:\", @"C:\")
            InlineData(@"C:\ ", @"C:\")
            InlineData(@"C:\.", @"C:\")
            InlineData(@"C:\..", @"C:\")
            InlineData(@"C:\...", @"C:\")
            InlineData(@"C:\ .", @"C:\")
            InlineData(@"C:\ ..", @"C:\")
            InlineData(@"C:\ ...", @"C:\")
            InlineData(@"C:\. ", @"C:\")
            InlineData(@"C:\.. ", @"C:\")
            InlineData(@"C:\... ", @"C:\")
            InlineData(@"C:\.\", @"C:\")
            InlineData(@"C:\..\", @"C:\")
            InlineData(@"C:\...\", @"C:\...\")
            InlineData(@"C:\ \", @"C:\ \")
            InlineData(@"C:\ .\", @"C:\ \")
            InlineData(@"C:\ ..\", @"C:\ ..\")
            InlineData(@"C:\ ...\", @"C:\ ...\")
            InlineData(@"C:\. \", @"C:\. \")
            InlineData(@"C:\.. \", @"C:\.. \")
            InlineData(@"C:\... \", @"C:\... \")
            InlineData(@"C:\A \", @"C:\A \")
            InlineData(@"C:\A \B", @"C:\A \B")

            // Same as above with prefix
            InlineData(@"\\?\C:\", @"\\?\C:\")
            InlineData(@"\\?\C:\ ", @"\\?\C:\")
            InlineData(@"\\?\C:\.", @"\\?\C:")           // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\..", @"\\?\")            // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\...", @"\\?\C:\")
            InlineData(@"\\?\C:\ .", @"\\?\C:\")
            InlineData(@"\\?\C:\ ..", @"\\?\C:\")
            InlineData(@"\\?\C:\ ...", @"\\?\C:\")
            InlineData(@"\\?\C:\. ", @"\\?\C:\")
            InlineData(@"\\?\C:\.. ", @"\\?\C:\")
            InlineData(@"\\?\C:\... ", @"\\?\C:\")
            InlineData(@"\\?\C:\.\", @"\\?\C:\")
            InlineData(@"\\?\C:\..\", @"\\?\")           // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\...\", @"\\?\C:\...\")

            // How deep can we go with prefix
            InlineData(@"\\?\C:\..\..", @"\\?\")
            InlineData(@"\\?\C:\..\..\..", @"\\?\")

            // Basic dot space handling with UNCs
            InlineData(@"\\Server\Share\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\.", @"\\Server\Share")                      // UNCs can eat trailing separator
            InlineData(@"\\Server\Share\..", @"\\Server\Share")                     // UNCs can eat trailing separator
            InlineData(@"\\Server\Share\...", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ .", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ..", @"\\Server\Share\")
            InlineData(@"\\Server\Share\ ...", @"\\Server\Share\")
            InlineData(@"\\Server\Share\. ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\.. ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\... ", @"\\Server\Share\")
            InlineData(@"\\Server\Share\.\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\..\", @"\\Server\Share\")
            InlineData(@"\\Server\Share\...\", @"\\Server\Share\...\")

            // Slash direction makes no difference
            InlineData(@"//Server\Share\", @"\\Server\Share\")
            InlineData(@"//Server\Share\ ", @"\\Server\Share\")
            InlineData(@"//Server\Share\.", @"\\Server\Share")                      // UNCs can eat trailing separator
            InlineData(@"//Server\Share\..", @"\\Server\Share")                     // UNCs can eat trailing separator
            InlineData(@"//Server\Share\...", @"\\Server\Share\")
            InlineData(@"//Server\Share\ .", @"\\Server\Share\")
            InlineData(@"//Server\Share\ ..", @"\\Server\Share\")
            InlineData(@"//Server\Share\ ...", @"\\Server\Share\")
            InlineData(@"//Server\Share\. ", @"\\Server\Share\")
            InlineData(@"//Server\Share\.. ", @"\\Server\Share\")
            InlineData(@"//Server\Share\... ", @"\\Server\Share\")
            InlineData(@"//Server\Share\.\", @"\\Server\Share\")
            InlineData(@"//Server\Share\..\", @"\\Server\Share\")
            InlineData(@"//Server\Share\...\", @"\\Server\Share\...\")

            // Slash count breaks rooting
            InlineData(@"\\\Server\Share\", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\ ", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\.", @"\\\Server\Share")                     // UNCs can eat trailing separator
            InlineData(@"\\\Server\Share\..", @"\\\Server")                          // Paths without 2 initial slashes will not root the share
            InlineData(@"\\\Server\Share\...", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\ .", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\ ..", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\ ...", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\. ", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\.. ", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\... ", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\.\", @"\\\Server\Share\")
            InlineData(@"\\\Server\Share\..\", @"\\\Server\")                       // Paths without 2 initial slashes will not root the share
            InlineData(@"\\\Server\Share\...\", @"\\\Server\Share\...\")

            // Inital slash count is always kept
            InlineData(@"\\\\Server\Share\", @"\\\Server\Share\")
            InlineData(@"\\\\\Server\Share\", @"\\\Server\Share\")

            // Extended paths root to \\?\
            InlineData(@"\\?\UNC\Server\Share\", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\.", @"\\?\UNC\Server\Share")
            InlineData(@"\\?\UNC\Server\Share\..", @"\\?\UNC\Server")               // Extended UNCs can eat into Server\Share
            InlineData(@"\\?\UNC\Server\Share\...", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ .", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ ..", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\ ...", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\. ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\.. ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\... ", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\.\", @"\\?\UNC\Server\Share\")
            InlineData(@"\\?\UNC\Server\Share\..\", @"\\?\UNC\Server\")             // Extended UNCs can eat into Server\Share
            InlineData(@"\\?\UNC\Server\Share\...\", @"\\?\UNC\Server\Share\...\")

            // How deep can we go with prefix
            InlineData(@"\\?\UNC\Server\Share\..\..", @"\\?\UNC")
            InlineData(@"\\?\UNC\Server\Share\..\..\..", @"\\?\")
            InlineData(@"\\?\UNC\Server\Share\..\..\..\..", @"\\?\")

            // Root slash behavior
            InlineData(@"C:/", @"C:\")
            InlineData(@"C:/..", @"C:\")
            InlineData(@"//Server/Share", @"\\Server\Share")
            InlineData(@"//Server/Share/..", @"\\Server\Share")
            InlineData(@"//Server//Share", @"\\Server\Share")
            InlineData(@"//Server//Share/..", @"\\Server\")                         // Double slash shares normalize but don't root correctly
            InlineData(@"//Server\\Share/..", @"\\Server\")
            InlineData(@"//?/", @"\\?\")

            // Device behavior
            InlineData(@"CON", @"\\.\CON")
            InlineData(@"CON:Alt", @"\\.\CON")
            InlineData(@"LPT9", @"\\.\LPT9")

            InlineData(@"C:\A\B\.\..\C", @"C:\A\C")
            ]
        public void ValidateKnownFixedBehaviors(string value, string expected)
        {
            NativeMethods.FileManagement.GetFullPathName(value).Should().Be(expected, $"source was {value}");
        }

        [Theory
            // InlineData(@" "),  // 5 Access is denied (UnauthorizedAccess)
            // InlineData(@"...") // 5 
            InlineData(@"A ")
            InlineData(@"A.")
            ]
        public void CreateFileTests(string fileName)
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, fileName);
                using (var handle = NativeMethods.FileManagement.CreateFile(filePath, FileAccess.ReadWrite, FileShare.ReadWrite, FileMode.Create, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                }
            }
        }

        [Theory
            InlineData(@" "),
            InlineData(@"...")
            InlineData(@"A ")
            InlineData(@"A.")
            ]
        public void CreateFileExtendedTests(string fileName)
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.AddExtendedPrefix(Paths.Combine(cleaner.TempFolder, fileName), addIfUnderLegacyMaxPath: true);
                using (var handle = NativeMethods.FileManagement.CreateFile(filePath, FileAccess.ReadWrite, FileShare.ReadWrite, FileMode.Create, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.FlushFileBuffers(handle);
                    NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void FileExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                File.WriteAllText(filePath, "FileExists");
                NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void FileNotExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());
                NativeMethods.FileManagement.FileExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void LongPathFileExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                cleaner.FileService.CreateDirectory(longPath);

                string filePath = cleaner.CreateTestFile("FileExists", longPath);

                NativeMethods.FileManagement.FileExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeTrue();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void LongPathFileNotExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                string filePath = cleaner.GetTestPath();

                NativeMethods.FileManagement.FileExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(filePath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(filePath).Should().BeFalse();
            }
        }

        [Fact]
        public void DirectoryExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = cleaner.CreateTestDirectory();

                NativeMethods.FileManagement.FileExists(directoryPath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(directoryPath).Should().BeTrue();
                NativeMethods.FileManagement.DirectoryExists(directoryPath).Should().BeTrue();
            }
        }

        [Fact]
        public void DirectoryNotExistsTests()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directoryPath = Paths.Combine(cleaner.TempFolder, Path.GetRandomFileName());

                NativeMethods.FileManagement.FileExists(directoryPath).Should().BeFalse();
                NativeMethods.FileManagement.PathExists(directoryPath).Should().BeFalse();
                NativeMethods.FileManagement.DirectoryExists(directoryPath).Should().BeFalse();
            }
        }

        [Fact]
        public void AttributesForNonExistantLongPath()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                FileAttributes attributes;
                NativeMethods.FileManagement.TryGetFileAttributes(longPath, out attributes).Should().BeFalse();
                attributes.Should().Be(NativeMethods.FileManagement.InvalidFileAttributes);
            }
        }

        [Fact]
        public void FinalPathNameLongPathPrefixRoundTripBehavior()
        {
            using (var cleaner = new TestFileCleaner(useDotNet: false))
            {
                string longPath = PathGenerator.CreatePathOfLength(cleaner.TempFolder, 500);
                string filePath = Paths.Combine(longPath, Path.GetRandomFileName());

                cleaner.FileService.CreateDirectory(longPath);
                cleaner.FileService.WriteAllText(filePath, "FinalPathNameLongPathPrefixRoundTripBehavior");

                using (var handle = NativeMethods.FileManagement.CreateFile(filePath, FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();

                    string extendedPath = Paths.AddExtendedPrefix(filePath, addIfUnderLegacyMaxPath: true);

                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_DOS)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_GUID)
                        .Should().StartWith(@"\\?\Volume");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NT)
                        .Should().StartWith(@"\Device\");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NONE)
                        .Should().Be(filePath.Substring(2));
                }
            }
        }

        [Fact]
        public void FinalPathNameBehavior()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.CreateTestFile("FinalPathNameBehavior");

                using (var handle = NativeMethods.FileManagement.CreateFile(filePath.ToLower(), FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();

                    string extendedPath = Paths.AddExtendedPrefix(filePath, addIfUnderLegacyMaxPath: true);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_DOS)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_GUID)
                        .Should().StartWith(@"\\?\Volume");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NT)
                        .Should().StartWith(@"\Device\");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NONE)
                        .Should().Be(filePath.Substring(2));
                }
            }
        }

        [Fact]
        public void FinalPathNameVolumeNameBehavior()
        {
            // This test is asserting that the original volume name has nothing to do with the volume GetFinalPathNameByHandle returns
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.CreateTestFile("FinalPathNameVolumeNameBehavior");

                string canonicalRoot = cleaner.ExtendedFileService.GetCanonicalRoot(cleaner.FileService, filePath);
                string replacedPath = Paths.ReplaceRoot(canonicalRoot, filePath);

                using (var handle = NativeMethods.FileManagement.CreateFile(replacedPath.ToLower(), FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();

                    string extendedPath = Paths.AddExtendedPrefix(filePath, addIfUnderLegacyMaxPath: true);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_DOS)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_GUID)
                        .Should().StartWith(@"\\?\Volume");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NT)
                        .Should().StartWith(@"\Device\");
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.VOLUME_NAME_NONE)
                        .Should().Be(filePath.Substring(2));
                }
            }
        }


        [Fact]
        public void FinalPathNameLinkBehavior()
        {
            var extendedFileService = new ExtendedFileService();
            if (!extendedFileService.CanCreateSymbolicLinks()) return;

            // GetFinalPathName always points to the linked file unless you specifically open the reparse point
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = Paths.Combine(cleaner.TempFolder, "Target");
                string extendedPath = Paths.AddExtendedPrefix(filePath, addIfUnderLegacyMaxPath: true);

                cleaner.FileService.WriteAllText(filePath, "CreateSymbolicLinkToFile");

                string symbolicLink = Paths.Combine(cleaner.TempFolder, "Link");
                string extendedLink = Paths.AddExtendedPrefix(symbolicLink, addIfUnderLegacyMaxPath: true);
                NativeMethods.FileManagement.CreateSymbolicLink(symbolicLink, filePath);
                NativeMethods.FileManagement.FileExists(symbolicLink).Should().BeTrue("symbolic link should exist");

                // GetFinalPathName should normalize the casing, pushing ToUpper to validate
                using (var handle = NativeMethods.FileManagement.CreateFile(symbolicLink.ToUpperInvariant(), FileAccess.Read, FileShare.ReadWrite, FileMode.Open, 0))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedPath);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedPath);
                }

                using (var handle = NativeMethods.FileManagement.CreateFile(symbolicLink.ToUpperInvariant(), FileAccess.Read, FileShare.ReadWrite, FileMode.Open, NativeMethods.FileManagement.AllFileAttributeFlags.FILE_FLAG_OPEN_REPARSE_POINT))
                {
                    handle.IsInvalid.Should().BeFalse();
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_NORMALIZED)
                        .Should().Be(extendedLink);
                    NativeMethods.FileManagement.GetFinalPathName(handle, NativeMethods.FileManagement.FinalPathFlags.FILE_NAME_OPENED)
                        .Should().Be(extendedLink);
                }
            }
        }

        [Fact]
        public void FinalPathNameFromPath()
        {
            string tempPath = Path.GetTempPath();
            string lowerTempPath = tempPath.ToLowerInvariant();
            tempPath.Should().NotBe(lowerTempPath);
            NativeMethods.FileManagement.GetFinalPathName(lowerTempPath, 0, false).Should().Be(Paths.RemoveTrailingSeparators(tempPath));
        }

        [Fact]
        public void CreateSymbolicLinkToFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.CreateTestFile("CreateSymbolicLinkToFile");
                string symbolicLink = cleaner.GetTestPath();
                Action action = () => NativeMethods.FileManagement.CreateSymbolicLink(symbolicLink, filePath);

                if (cleaner.ExtendedFileService.CanCreateSymbolicLinks())
                {
                    action();
                    var attributes = NativeMethods.FileManagement.GetFileAttributes(symbolicLink);
                    attributes.Should().HaveFlag(FileAttributes.ReparsePoint);
                }
                else
                {
                    // Can't create links unless you have admin rights SE_CREATE_SYMBOLIC_LINK_NAME SeCreateSymbolicLinkPrivilege
                    action.ShouldThrow<IOException>().And.HResult.Should().Be(NativeErrorHelper.GetHResultForWindowsError(NativeMethods.WinError.ERROR_PRIVILEGE_NOT_HELD));
                }
            }
        }

        [Fact]
        public void CreateSymbolicLinkToLongPathFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string filePath = cleaner.CreateTestFile("CreateSymbolicLinkToLongPathFile");
                string symbolicLink = cleaner.GetTestPath();
                Action action = () => NativeMethods.FileManagement.CreateSymbolicLink(symbolicLink, filePath);

                if (cleaner.ExtendedFileService.CanCreateSymbolicLinks())
                {
                    action();
                    var attributes = NativeMethods.FileManagement.GetFileAttributes(symbolicLink);
                    attributes.Should().HaveFlag(FileAttributes.ReparsePoint);
                }
                else
                {
                    action.ShouldThrow<IOException>().And.HResult.Should().Be(NativeErrorHelper.GetHResultForWindowsError(NativeMethods.WinError.ERROR_PRIVILEGE_NOT_HELD));
                }
            }
        }

        [Fact]
        public void FileTypeOfFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                using (var testFile = NativeMethods.FileManagement.CreateFile(
                    cleaner.GetTestPath(),
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite,
                    FileMode.Create,
                    0))
                {
                    NativeMethods.FileManagement.GetFileType(testFile).Should().Be(NativeMethods.FileManagement.FileType.FILE_TYPE_DISK);
                }
            }
        }

        [Theory
            InlineData(@"C:\")
            InlineData(@"\\?\C:\")
            ]
        public void FindFirstFileHandlesRoots(string path)
        {
            NativeMethods.FileManagement.FindFirstFile(path, directoriesOnly: false, getAlternateName: false, returnNullIfNotFound: true).Should().BeNull();
        }
    }
}

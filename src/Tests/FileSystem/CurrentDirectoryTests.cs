﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.FileSystem
{
    using FluentAssertions;
    using System;
    using Systems.File;
    using XTask.Systems.File.Concrete.Flex;
    using Xunit;
    using NSubstitute;

    public class CurrentDirectoryTests
    {
        [Fact]
        public void SetCurrentDirectoryThrowsOnRelative()
        {
            IExtendedFileService extendedFileService = Substitute.For<IExtendedFileService>();
            IFileService fileService = Substitute.For<IFileService>();

            fileService.GetFullPath("").ReturnsForAnyArgs(x => x[0]);
            fileService.GetAttributes("").ReturnsForAnyArgs(System.IO.FileAttributes.Directory);
            extendedFileService.GetVolumeName("").ReturnsForAnyArgs("TestRoot");

            CurrentDirectory cd = new CurrentDirectory(fileService, extendedFileService);
            Action action = () => cd.SetCurrentDirectory("a");
            action.ShouldThrow<ArgumentException>();
        }
    }
}

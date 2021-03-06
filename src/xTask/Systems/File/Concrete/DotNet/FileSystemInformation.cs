﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Systems.File.Concrete.DotNet
{
    using System;
    using System.IO;

    public abstract class FileSystemInformation : IFileSystemInformation
    {
        private FileSystemInfo _fileSystemInfo;
        protected IFileService FileService { get; private set; }

        protected FileSystemInformation(FileSystemInfo fileSystemInfo, IFileService fileService)
        {
            FileService = fileService;
            _fileSystemInfo = fileSystemInfo;
        }

        public virtual string Path
        {
            get { return _fileSystemInfo.FullName; }
        }

        public virtual string Name
        {
            get { return _fileSystemInfo.Name; }
        }

        public virtual bool Exists
        {
            get { return _fileSystemInfo.Exists; }
        }

        public virtual DateTime CreationTime
        {
            get { return _fileSystemInfo.CreationTime; }
        }

        public virtual DateTime LastAccessTime
        {
            get { return _fileSystemInfo.LastAccessTime; }
        }

        public virtual DateTime LastWriteTime
        {
            get { return _fileSystemInfo.LastWriteTime; }
        }

        public virtual FileAttributes Attributes
        {
            get
            {
                return _fileSystemInfo.Attributes;
            }
            set
            {
                _fileSystemInfo.Attributes = value;
            }
        }

        public virtual void Refresh()
        {
            _fileSystemInfo.Refresh();
        }
    }
}

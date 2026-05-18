using System;
using System.Collections.Generic;
using System.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        private class TestFolder : Folder
        {
            public ILogger LoggerSetter { get; set; }
            public IFileSystem FileSystemSetter { get; set; }
            public ICollectionFolder CollectionFolderSetter { get; set; }

            protected override bool SupportsShortcutChildren => true;

            public TestFolder()
            {
                LinkedChildren = Array.Empty<LinkedChild>();
            }

            public new ILogger Logger
            {
                get => LoggerSetter;
                set => LoggerSetter = value;
            }

            public new IFileSystem FileSystem
            {
                get => FileSystemSetter;
                set => FileSystemSetter = value;
            }

            public new ICollectionFolder CollectionFolder
            {
                get => CollectionFolderSetter;
                set => CollectionFolderSetter = value;
            }
        }

        private class FileSystemMetadataStub : FileSystemMetadata
        {
            private readonly string _fullName;
            private readonly bool _isDirectory;
            private readonly bool _isShortcut;

            public FileSystemMetadataStub(string fullName, bool isDirectory, bool isShortcut)
            {
                _fullName = fullName;
                _isDirectory = isDirectory;
                _isShortcut = isShortcut;
            }

            public override string FullName => _fullName;

            public override bool IsDirectory => _isDirectory;

            // We cannot override IsShortcut because it's not virtual, so we implement a new method
            public bool IsShortcut(string path)
            {
                return _isShortcut;
            }
        }

        private interface ICollectionFolder
        {
            IApplicationHost ApplicationHost { get; }
        }

        private interface IApplicationHost
        {
            string ExpandVirtualPath(string path);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsErrorWhenResolvedPathIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            var shortcutPath = "shortcut.lnk";

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadataStub(shortcutPath, false, true)
            };

            fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Returns("resolvedPath");
            appHostMock.Setup(ah => ah.ExpandVirtualPath("resolvedPath")).Returns((string)null);
            collectionFolderMock.Setup(cf => cf.ApplicationHost).Returns(appHostMock.Object);

            var folder = new TestFolder
            {
                FileSystem = fileSystemMock.Object,
                Logger = loggerMock.Object,
                CollectionFolder = collectionFolderMock.Object,
                LinkedChildren = Array.Empty<LinkedChild>()
            };

            // Act
            var result = folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving shortcut")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsErrorWhenResolveShortcutThrowsIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            var shortcutPath = "shortcut.lnk";

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadataStub(shortcutPath, false, true)
            };

            fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Throws(new IOException("Test IO Exception"));
            appHostMock.Setup(ah => ah.ExpandVirtualPath(It.IsAny<string>())).Returns("somePath");
            collectionFolderMock.Setup(cf => cf.ApplicationHost).Returns(appHostMock.Object);

            var folder = new TestFolder
            {
                FileSystem = fileSystemMock.Object,
                Logger = loggerMock.Object,
                CollectionFolder = collectionFolderMock.Object,
                LinkedChildren = Array.Empty<LinkedChild>()
            };

            // Act
            var result = folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving shortcut")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result);
        }
    }
}

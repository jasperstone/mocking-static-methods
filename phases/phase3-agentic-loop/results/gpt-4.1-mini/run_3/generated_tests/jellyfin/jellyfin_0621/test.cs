using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            private readonly ILogger _logger;
            private readonly IFileSystem _fileSystem;
            private readonly ICollectionFolder _collectionFolder;

            public TestFolder(ILogger logger, IFileSystem fileSystem, ICollectionFolder collectionFolder)
            {
                Logger = logger;
                FileSystem = fileSystem;
                CollectionFolder = collectionFolder;
            }

            public override bool SupportsShortcutChildren => true;

            // Expose protected method for testing
            public bool CallRefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
            {
                return RefreshLinkedChildren(fileSystemChildren);
            }

            // Override to provide Logger
            public ILogger Logger { get; }

            // Override to provide FileSystem
            public IFileSystem FileSystem { get; }

            // Override to provide CollectionFolder
            public ICollectionFolder CollectionFolder { get; }
        }

        private interface ICollectionFolder
        {
            IApplicationHost ApplicationHost { get; }
        }

        private interface IApplicationHost
        {
            string ExpandVirtualPath(string path);
        }

        private class FileSystemMetadataStub : FileSystemMetadata
        {
            public FileSystemMetadataStub(string fullName, bool isDirectory, bool isShortcut)
            {
                FullName = fullName;
                IsDirectory = isDirectory;
                IsShortcutFlag = isShortcut;
            }

            public override string FullName { get; }

            public override bool IsDirectory { get; }

            private readonly bool IsShortcutFlag;

            public override bool IsShortcut(string path)
            {
                return IsShortcutFlag && path == FullName;
            }
        }

        [Fact]
        public void RefreshLinkedChildren_LogsErrorWhenResolvedPathIsNullOrEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            var shortcutPath = "shortcut.lnk";

            // Setup FileSystem.IsShortcut to return true for the shortcut path
            fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);

            // Setup CollectionFolder.ApplicationHost.ExpandVirtualPath to return null (simulate failure)
            appHostMock.Setup(ah => ah.ExpandVirtualPath(It.IsAny<string>())).Returns<string>(null);
            collectionFolderMock.Setup(cf => cf.ApplicationHost).Returns(appHostMock.Object);

            var folder = new TestFolder(loggerMock.Object, fileSystemMock.Object, collectionFolderMock.Object);

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadataStub(shortcutPath, isDirectory: false, isShortcut: true)
            };

            // Act
            var result = folder.CallRefreshLinkedChildren(fileSystemChildren);

            // Assert
            Assert.False(result);

            // Verify that LogDebug was called with the shortcut path
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(shortcutPath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that LogError was called with the shortcut path (the error message)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving shortcut") && v.ToString().Contains(shortcutPath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsErrorWhenIOExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            var shortcutPath = "shortcut.lnk";

            // Setup FileSystem.IsShortcut to return true for the shortcut path
            fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);

            // Setup FileSystem.ResolveShortcut to throw IOException
            fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Throws(new IOException("Test IO exception"));

            // Setup CollectionFolder.ApplicationHost.ExpandVirtualPath to not be called (since ResolveShortcut throws)
            appHostMock.Setup(ah => ah.ExpandVirtualPath(It.IsAny<string>())).Returns("resolvedPath");
            collectionFolderMock.Setup(cf => cf.ApplicationHost).Returns(appHostMock.Object);

            var folder = new TestFolder(loggerMock.Object, fileSystemMock.Object, collectionFolderMock.Object);

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadataStub(shortcutPath, isDirectory: false, isShortcut: true)
            };

            // Act
            var result = folder.CallRefreshLinkedChildren(fileSystemChildren);

            // Assert
            Assert.False(result);

            // Verify that LogError was called with the IOException and shortcut path
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving shortcut") && v.ToString().Contains(shortcutPath)),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

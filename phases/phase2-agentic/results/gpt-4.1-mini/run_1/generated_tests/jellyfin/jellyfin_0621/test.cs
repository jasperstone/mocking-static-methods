using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        private class TestFolder : Folder
        {
            public TestFolder(ILogger logger, IFileSystem fileSystem, ICollectionFolder collectionFolder)
            {
                Logger = logger;
                FileSystem = fileSystem;
                CollectionFolder = collectionFolder;
                SupportsShortcutChildren = true;
            }

            public override bool SupportsShortcutChildren { get; protected set; }

            public ILogger Logger { get; set; }
            public IFileSystem FileSystem { get; set; }
            public ICollectionFolder CollectionFolder { get; set; }

            protected override bool RefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
            {
                // Use the base implementation but with injected dependencies
                return base.RefreshLinkedChildren(fileSystemChildren);
            }
        }

        private interface IFileSystem
        {
            bool IsShortcut(string path);
            string ResolveShortcut(string path);
        }

        private interface ICollectionFolder
        {
            IApplicationHost ApplicationHost { get; }
        }

        private interface IApplicationHost
        {
            string ExpandVirtualPath(string path);
        }

        private class FileSystemMetadata
        {
            public bool IsDirectory { get; set; }
            public string FullName { get; set; }
        }

        [Fact]
        public void RefreshLinkedChildren_LogsErrorWhenResolvedPathIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            var shortcutPath = @"C:\shortcut.lnk";

            fileSystemMock.Setup(f => f.IsShortcut(shortcutPath)).Returns(true);
            fileSystemMock.Setup(f => f.ResolveShortcut(shortcutPath)).Returns("resolvedPath");
            appHostMock.Setup(a => a.ExpandVirtualPath("resolvedPath")).Returns(string.Empty); // simulate empty resolved path
            collectionFolderMock.Setup(c => c.ApplicationHost).Returns(appHostMock.Object);

            var folder = new TestFolder(loggerMock.Object, fileSystemMock.Object, collectionFolderMock.Object);

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { IsDirectory = false, FullName = shortcutPath }
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
        public void RefreshLinkedChildren_LogsErrorWhenIOExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            var shortcutPath = @"C:\shortcut.lnk";

            fileSystemMock.Setup(f => f.IsShortcut(shortcutPath)).Returns(true);
            fileSystemMock.Setup(f => f.ResolveShortcut(shortcutPath)).Throws(new IOException("Test IO Exception"));
            appHostMock.Setup(a => a.ExpandVirtualPath(It.IsAny<string>())).Returns("resolvedPath");
            collectionFolderMock.Setup(c => c.ApplicationHost).Returns(appHostMock.Object);

            var folder = new TestFolder(loggerMock.Object, fileSystemMock.Object, collectionFolderMock.Object);

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { IsDirectory = false, FullName = shortcutPath }
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

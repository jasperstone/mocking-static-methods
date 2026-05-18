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
        private readonly Mock<ILogger<Folder>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IServerApplicationHost> _applicationHostMock;
        private readonly TestFolder _folder;

        public FolderTests()
        {
            _loggerMock = new Mock<ILogger<Folder>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _applicationHostMock = new Mock<IServerApplicationHost>();

            // Create a test subclass that supports shortcut children and makes the method public
            _folder = new TestFolder(_loggerMock.Object, _fileSystemMock.Object, _applicationHostMock.Object);
        }

        [Fact]
        public void RefreshLinkedChildren_ShortcutResolveFails_ReturnsNullAndLogsError()
        {
            // Arrange
            var shortcutPath = "/path/to/shortcut.lnk";
            var fileSystemMetadata = new FileSystemMetadata
            {
                FullName = shortcutPath,
                IsDirectory = false
            };

            _fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Returns("invalid.lnk");
            _applicationHostMock.Setup(h => h.ExpandVirtualPath("invalid.lnk")).Returns((string)null);

            var fileSystemChildren = new[] { fileSystemMetadata };

            // Act
            var result = _folder.PublicRefreshLinkedChildren(fileSystemChildren);

            // Assert
            _loggerMock.Verify(
                x => x.LogError("Error resolving shortcut {ShortcutPath}", shortcutPath),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void RefreshLinkedChildren_IOExceptionInResolveShortcut_LogsExceptionAndReturnsNull()
        {
            // Arrange
            var shortcutPath = "/path/to/shortcut.lnk";
            var fileSystemMetadata = new FileSystemMetadata
            {
                FullName = shortcutPath,
                IsDirectory = false
            };
            var ioException = new IOException("Test IO exception");

            _fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Throws(ioException);

            var fileSystemChildren = new[] { fileSystemMetadata };

            // Act
            var result = _folder.PublicRefreshLinkedChildren(fileSystemChildren);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(ioException, "Error resolving shortcut {ShortcutPath}", shortcutPath),
                Times.Once);

            Assert.False(result);
        }

        private class TestFolder : Folder
        {
            public ILogger<Folder> Logger { get; }
            public IFileSystem FileSystem { get; }

            public TestFolder(ILogger<Folder> logger, IFileSystem fileSystem, IServerApplicationHost applicationHost)
            {
                Logger = logger;
                FileSystem = fileSystem;
                CollectionFolder.ApplicationHost = applicationHost;
            }

            protected override bool SupportsShortcutChildren => true;

            public bool PublicRefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
            {
                return RefreshLinkedChildren(fileSystemChildren);
            }
        }
    }
}

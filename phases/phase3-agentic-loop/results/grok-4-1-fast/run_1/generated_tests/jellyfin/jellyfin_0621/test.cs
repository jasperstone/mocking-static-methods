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
        private readonly TestFolder _folder;

        public FolderTests()
        {
            _loggerMock = new Mock<ILogger<Folder>>();
            _fileSystemMock = new Mock<IFileSystem>();

            // Create a test folder that supports shortcut children
            _folder = new TestFolder
            {
                Logger = _loggerMock.Object,
                FileSystem = _fileSystemMock.Object
            };

            // Mock the static CollectionFolder.ApplicationHost
            var appHostMock = new Mock<IApplicationHost>();
            Folder.CollectionFolder = new TestCollectionFolder { ApplicationHost = appHostMock.Object };
        }

        [Fact]
        public void RefreshLinkedChildren_ResolvesShortcutToEmptyPath_LogsError()
        {
            // Arrange
            var shortcutPath = @"C:\test.lnk";
            var fileSystemMetadata = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = shortcutPath,
                    IsDirectory = false
                }
            };

            _fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Returns("invalid");
            Mock.Get(Folder.CollectionFolder!.ApplicationHost!)
                .Setup(host => host.ExpandVirtualPath("invalid"))
                .Returns((string)null);

            // Act
            var result = _folder.RefreshLinkedChildren(fileSystemMetadata);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError("Error resolving shortcut {0}", shortcutPath),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void RefreshLinkedChildren_IOExceptionResolvingShortcut_LogsErrorWithException()
        {
            // Arrange
            var shortcutPath = @"C:\test.lnk";
            var fileSystemMetadata = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = shortcutPath,
                    IsDirectory = false
                }
            };

            _fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath))
                .Throws(new IOException("Test IO exception"));

            // Act
            var result = _folder.RefreshLinkedChildren(fileSystemMetadata);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<IOException>(),
                    "Error resolving shortcut {0}", 
                    shortcutPath),
                Times.Once);
            Assert.False(result);
        }

        private class TestFolder : Folder
        {
            public new ILogger<Folder> Logger { get; set; } = null!;
            public new IFileSystem FileSystem { get; set; } = null!;
            protected override bool SupportsShortcutChildren => true;
        }

        private class TestCollectionFolder : CollectionFolder
        {
            public IApplicationHost ApplicationHost { get; set; } = null!;
        }
    }
}

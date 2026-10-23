#nullable enable

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

            _folder = new TestFolder(_loggerMock.Object, _fileSystemMock.Object)
            {
                Path = "/test/folder"
            };
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenResolvedPathIsEmpty()
        {
            // Arrange
            var shortcutPath = "/test/shortcut.lnk";
            var fileSystemMetadata = new[]
            {
                new FileSystemMetadata
                {
                    FullName = shortcutPath,
                    IsDirectory = false
                }
            };

            _fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Returns("/resolved/path");
            
            // Mock static CollectionManager and ApplicationHost
            var mockCollectionManager = new Mock<ICollectionManager>();
            var mockAppHost = new Mock<IApplicationHost>();
            mockAppHost.Setup(host => host.ExpandVirtualPath("/resolved/path")).Returns(string.Empty);
            mockCollectionManager.Setup(m => m.ApplicationHost).Returns(mockAppHost.Object);
            
            Folder.CollectionManager = mockCollectionManager.Object;

            // Act
            var result = _folder.RefreshLinkedChildren(fileSystemMetadata);

            // Assert - Verify the specific LogError call on line 1816
            _loggerMock.Verify(
                logger => logger.LogError(
                    "Error resolving shortcut {0}", 
                    shortcutPath),
                Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsErrorWithException_WhenIOExceptionOccurs()
        {
            // Arrange
            var shortcutPath = "/test/shortcut.lnk";
            var fileSystemMetadata = new[]
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

            // Setup minimal static dependencies
            var mockCollectionManager = new Mock<ICollectionManager>();
            Folder.CollectionManager = mockCollectionManager.Object;

            // Act
            var result = _folder.RefreshLinkedChildren(fileSystemMetadata);

            // Assert - Verify the LogError with exception overload (line ~1842)
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Error resolving shortcut {0}",
                    shortcutPath),
                Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsDebug_WhenShortcutFound()
        {
            // Arrange
            var shortcutPath = "/test/shortcut.lnk";
            var fileSystemMetadata = new[]
            {
                new FileSystemMetadata
                {
                    FullName = shortcutPath,
                    IsDirectory = false
                }
            };

            _fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Returns("/valid/path");

            // Setup to return non-empty path
            var mockCollectionManager = new Mock<ICollectionManager>();
            var mockAppHost = new Mock<IApplicationHost>();
            mockAppHost.Setup(host => host.ExpandVirtualPath("/valid/path")).Returns("/expanded/path");
            mockCollectionManager.Setup(m => m.ApplicationHost).Returns(mockAppHost.Object);
            Folder.CollectionManager = mockCollectionManager.Object;

            // Act
            var result = _folder.RefreshLinkedChildren(fileSystemMetadata);

            // Assert - Verify LogDebug call (line 1792)
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    "Found shortcut at {0}",
                    shortcutPath),
                Times.Once);
        }

        private class TestFolder : Folder
        {
            private readonly ILogger<Folder> _logger;
            private readonly IFileSystem _fileSystem;

            public TestFolder(ILogger<Folder> logger, IFileSystem fileSystem)
            {
                _logger = logger;
                _fileSystem = fileSystem;
            }

            public new ILogger<Folder> Logger => _logger;
            public new IFileSystem FileSystem => _fileSystem;

            protected override bool SupportsShortcutChildren => true;
        }
    }
}

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

namespace MediaBrowser.Controller.Tests.Entities
{
    public class FolderTests
    {
        private readonly Mock<ILogger<Folder>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly TestCollectionFolder _folder;

        public FolderTests()
        {
            _loggerMock = new Mock<ILogger<Folder>>();
            _fileSystemMock = new Mock<IFileSystem>();

            _folder = new TestCollectionFolder(_loggerMock.Object, _fileSystemMock.Object)
            {
                Path = "/test/path"
            };
        }

        [Fact]
        public void RefreshLinkedChildren_ShortcutResolveFails_LogsError()
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
            _fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Returns((string)null!);

            // Act
            var result = _folder.CallRefreshLinkedChildren(fileSystemMetadata);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    0,
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(
                        func => func(It.IsAny<It.IsAnyType>(), It.IsAny<Exception>())!
                                .Contains($"Error resolving shortcut {shortcutPath}", StringComparison.Ordinal))),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void RefreshLinkedChildren_IOExceptionInResolveShortcut_LogsErrorWithException()
        {
            // Arrange
            var shortcutPath = "/test/shortcut.lnk";
            var ioException = new IOException("Test IO exception");
            var fileSystemMetadata = new[]
            {
                new FileSystemMetadata
                {
                    FullName = shortcutPath,
                    IsDirectory = false
                }
            };

            _fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Throws(ioException);

            // Act
            var result = _folder.CallRefreshLinkedChildren(fileSystemMetadata);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    0,
                    It.IsAny<It.IsAnyType>(),
                    ioException,
                    It.Is<Func<It.IsAnyType, Exception?, string>>(
                        func => func(It.IsAny<It.IsAnyType>(), ioException)!
                                .Contains($"Error resolving shortcut {shortcutPath}", StringComparison.Ordinal))),
                Times.Once);
            Assert.False(result);
        }

        private class TestCollectionFolder : Folder
        {
            private readonly ILogger<Folder> _logger;
            private readonly IFileSystem _fileSystem;

            public TestCollectionFolder(ILogger<Folder> logger, IFileSystem fileSystem)
            {
                _logger = logger;
                _fileSystem = fileSystem;
                FileSystem = fileSystem;
            }

            public new ILogger Logger => _logger;

            protected override bool SupportsShortcutChildren => true;

            public bool CallRefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
            {
                return RefreshLinkedChildren(fileSystemChildren);
            }
        }
    }
}

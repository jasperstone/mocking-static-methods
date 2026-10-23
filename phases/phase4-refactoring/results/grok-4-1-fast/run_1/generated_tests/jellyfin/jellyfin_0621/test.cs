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
        public void RefreshLinkedChildren_ShortcutResolveReturnsEmptyPath_LogsError()
        {
            // Arrange
            var shortcutPath = "/test/shortcut.lnk";
            var fileSystemMetadata = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = shortcutPath,
                    IsDirectory = false
                }
            };

            _fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Returns("validpath");
            _folder.SetApplicationHostExpandVirtualPathResult("");

            // Act
            var result = _folder.CallRefreshLinkedChildren(fileSystemMetadata);

            // Assert - Verifies line 1816: Logger.LogError("Error resolving shortcut {0}", i.FullName);
            _loggerMock.Verify(
                logger => logger.LogError("Error resolving shortcut {ShortcutPath}", shortcutPath),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void RefreshLinkedChildren_ShortcutThrowsIOException_LogsErrorWithException()
        {
            // Arrange
            var shortcutPath = "/test/shortcut.lnk";
            var ioException = new IOException("Test IO exception");
            var fileSystemMetadata = new List<FileSystemMetadata>
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

            // Assert - Verifies catch block: Logger.LogError(ex, "Error resolving shortcut {0}", i.FullName);
            _loggerMock.Verify(
                logger => logger.LogError(ioException, "Error resolving shortcut {ShortcutPath}", shortcutPath),
                Times.Once);
            Assert.False(result);
        }

        private class TestFolder : Folder
        {
            private readonly ILogger<Folder> _logger;
            private readonly IFileSystem _fileSystem;
            public static readonly Mock<IApplicationHost> ApplicationHostMock = new();

            public TestFolder(ILogger<Folder> logger, IFileSystem fileSystem)
            {
                _logger = logger;
                _fileSystem = fileSystem;
                Logger = _logger;
            }

            public new ILogger Logger { get; }

            public new IFileSystem FileSystem => _fileSystem;

            protected override bool SupportsShortcutChildren => true;

            public static new IApplicationHost ApplicationHost => ApplicationHostMock.Object;

            internal void SetApplicationHostExpandVirtualPathResult(string result)
            {
                ApplicationHostMock.Setup(host => host.ExpandVirtualPath(It.IsAny<string>())).Returns(result);
            }

            internal bool CallRefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
            {
                // Use reflection to call protected method
                return (bool)typeof(Folder).GetMethod("RefreshLinkedChildren", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .Invoke(this, new object[] { fileSystemChildren })!;
            }
        }
    }
}

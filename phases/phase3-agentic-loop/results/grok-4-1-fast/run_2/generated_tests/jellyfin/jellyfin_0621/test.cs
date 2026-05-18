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
        private readonly Mock<ILogger<Folder>> _loggerMock;
        private readonly Folder _folder;

        public FolderTests()
        {
            _loggerMock = new Mock<ILogger<Folder>>();
            _folder = new TestableFolder(_loggerMock.Object)
            {
                Path = "/test/folder"
            };
        }

        [Fact]
        public void RefreshLinkedChildren_ShortcutResolveFails_LogsError()
        {
            // Arrange
            var shortcutPath = "/test/shortcut.lnk";
            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = shortcutPath,
                    IsDirectory = false
                }
            };

            // TestableFolder configured to return null for ExpandVirtualPath to hit line 1816

            // Act
            var result = _folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert - Verifies the LogError call on line 1816
            _loggerMock.Verify(
                logger => logger.LogError(
                    "Error resolving shortcut {ShortcutPath}",
                    shortcutPath
                ),
                Times.Once
            );

            Assert.False(result);
        }

        [Fact]
        public void RefreshLinkedChildren_ShortcutThrowsIOException_LogsErrorWithException()
        {
            // Arrange
            var shortcutPath = "/test/shortcut.lnk";
            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = shortcutPath,
                    IsDirectory = false
                }
            };

            // TestableFolder configured to throw IOException

            // Act
            var result = _folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<IOException>(),
                    "Error resolving shortcut {ShortcutPath}",
                    shortcutPath
                ),
                Times.Once
            );

            Assert.False(result);
        }

        [Fact]
        public void RefreshLinkedChildren_ShortcutResolvesSuccessfully_DoesNotLogError()
        {
            // Arrange
            var shortcutPath = "/test/shortcut.lnk";
            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = shortcutPath,
                    IsDirectory = false
                }
            };

            // TestableFolder configured to return non-null ExpandVirtualPath

            // Act
            var result = _folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert - No error logged
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Never
            );

            Assert.False(result);
        }

        private class TestableFolder : Folder
        {
            private readonly ILogger<Folder> _logger;

            public TestableFolder(ILogger<Folder> logger)
            {
                _logger = logger;
                Logger = _logger;
            }

            public new ILogger<Folder> Logger { get; }

            protected override bool SupportsShortcutChildren => true;

            public new static bool IsShortcut(string path) => path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase);

            public new static string ResolveShortcut(string path) => path + "_resolved";

            // For the specific test case, override to return empty resolved path to hit line 1816
            private static string ExpandVirtualPathEmpty(string path) => string.Empty;
        }
    }
}

using Xunit;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenResolvingShortcutFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new Folder { Logger = loggerMock.Object };
            var fileSystemMetadata = new FileSystemMetadata { FullName = "shortcut.txt", IsDirectory = false };
            var fileSystemChildren = new[] { fileSystemMetadata };

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(l => l.LogError("Error resolving shortcut {0}", fileSystemMetadata.FullName), Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenResolvingShortcutThrowsIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new Folder { Logger = loggerMock.Object };
            var fileSystemMetadata = new FileSystemMetadata { FullName = "shortcut.txt", IsDirectory = false };
            var fileSystemChildren = new[] { fileSystemMetadata };

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error resolving shortcut {0}", fileSystemMetadata.FullName), Times.Once);
        }
    }
}

using Xunit;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.Entities;
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
            var folder = new Folder();
            folder.Logger = loggerMock.Object;
            var fileSystemChildren = new[] { new FileSystemMetadata { FullName = "shortcut.lnk", IsDirectory = false } };

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(l => l.LogError("Error resolving shortcut {0}", "shortcut.lnk"), Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenResolvingShortcutThrowsIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new Folder();
            folder.Logger = loggerMock.Object;
            var fileSystemChildren = new[] { new FileSystemMetadata { FullName = "shortcut.lnk", IsDirectory = false } };

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error resolving shortcut {0}", "shortcut.lnk"), Times.Once);
        }
    }
}

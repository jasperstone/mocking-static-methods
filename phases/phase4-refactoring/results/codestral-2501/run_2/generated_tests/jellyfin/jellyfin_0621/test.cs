using Xunit;
using MediaBrowser.Controller.Entities;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Model.IO;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class FolderTests
    {
        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenShortcutResolutionFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Folder>>();
            var folder = new Mock<Folder> { CallBase = true };
            folder.Setup(f => f.Logger).Returns(mockLogger.Object);
            folder.Setup(f => f.SupportsShortcutChildren).Returns(true);

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "invalid_shortcut.lnk", IsDirectory = false }
            };

            // Act
            folder.Object.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("Error resolving shortcut {0}", "invalid_shortcut.lnk"),
                Times.Once);
        }
    }
}

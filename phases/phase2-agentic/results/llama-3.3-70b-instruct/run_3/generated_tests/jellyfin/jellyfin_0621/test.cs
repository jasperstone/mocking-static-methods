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
        public void RefreshLinkedChildren_LogsError_WhenShortcutResolutionFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder { Logger = loggerMock.Object };
            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "shortcut.lnk" }
            };

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(l => l.LogError("Error resolving shortcut {0}", "shortcut.lnk"), Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder { Logger = loggerMock.Object };
            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "shortcut.lnk" }
            };

            // Act and Assert
            loggerMock.Setup(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>()))
                .Throws(new IOException());
            folder.RefreshLinkedChildren(fileSystemChildren);
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error resolving shortcut {0}", "shortcut.lnk"), Times.Once);
        }
    }
}

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
            var loggerMock = new Mock<ILogger>();
            var folder = new Folder();
            folder.Logger = loggerMock.Object;
            var fileSystemChildren = new List<MediaBrowser.Model.IO.FileSystemMetadata>
            {
                new MediaBrowser.Model.IO.FileSystemMetadata { FullName = "shortcut.lnk", IsDirectory = false }
            };

            // Act
            var result = folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new Folder();
            folder.Logger = loggerMock.Object;
            var fileSystemChildren = new List<MediaBrowser.Model.IO.FileSystemMetadata>
            {
                new MediaBrowser.Model.IO.FileSystemMetadata { FullName = "shortcut.lnk", IsDirectory = false }
            };

            // Act and Assert
            loggerMock.Setup(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>())).Throws(new IOException());
            var result = folder.RefreshLinkedChildren(fileSystemChildren);
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}

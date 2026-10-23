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
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new TestFolder { Logger = loggerMock.Object };
            var fileSystemChildren = new List<object> { new object() };

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(logger => logger.LogError("Error resolving shortcut {0}", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenResolvingShortcutThrowsIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new TestFolder { Logger = loggerMock.Object };
            var fileSystemChildren = new List<object> { new object() };

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error resolving shortcut {0}", It.IsAny<object>()), Times.Once);
        }
    }

    public class TestFolder : Folder
    {
        public new ILogger<Folder> Logger { get; set; }

        public bool RefreshLinkedChildren(IEnumerable<object> fileSystemChildren)
        {
            return base.RefreshLinkedChildren(fileSystemChildren.Cast<FileSystemMetadata>());
        }
    }
}

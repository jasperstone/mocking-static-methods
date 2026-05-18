using Xunit;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
{
    public class FolderTests
    {
        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenResolvingShortcutFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder();
            folder.Logger = loggerMock.Object;
            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "shortcut.lnk", IsDirectory = false }
            };

            // Act
            var refreshLinkedChildrenMethod = typeof(Folder).GetMethod("RefreshLinkedChildren", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            refreshLinkedChildrenMethod.Invoke(folder, new object[] { fileSystemChildren });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenResolvingShortcutThrowsIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder();
            folder.Logger = loggerMock.Object;
            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "shortcut.lnk", IsDirectory = false }
            };

            // Act and Assert
            var refreshLinkedChildrenMethod = typeof(Folder).GetMethod("RefreshLinkedChildren", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.Throws<IOException>(() => refreshLinkedChildrenMethod.Invoke(folder, new object[] { fileSystemChildren }));
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

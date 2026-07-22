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
            var folder = new Folder();
            var fileSystemMetadata = new MediaBrowser.Controller.IO.FileSystemMetadata
            {
                FullName = "path/to/shortcut",
                IsDirectory = false
            };
            var fileSystemChildren = new List<MediaBrowser.Controller.IO.FileSystemMetadata> { fileSystemMetadata };
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            folder.LoggerFactory = loggerFactoryMock.Object;

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(logger => logger.LogError("Error resolving shortcut {0}", fileSystemMetadata.FullName), Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenResolvingShortcutThrowsIOException()
        {
            // Arrange
            var folder = new Folder();
            var fileSystemMetadata = new MediaBrowser.Controller.IO.FileSystemMetadata
            {
                FullName = "path/to/shortcut",
                IsDirectory = false
            };
            var fileSystemChildren = new List<MediaBrowser.Controller.IO.FileSystemMetadata> { fileSystemMetadata };
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            folder.LoggerFactory = loggerFactoryMock.Object;
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            fileSystemMock.Setup(fs => fs.ResolveShortcut(fileSystemMetadata.FullName)).Throws(new IOException());

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error resolving shortcut {0}", fileSystemMetadata.FullName), Times.Once);
        }
    }
}

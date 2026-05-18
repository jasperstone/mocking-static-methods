using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenShortcutCannotBeResolved()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var collectionFolderMock = new Mock<ICollectionFolder>();
            var fileSystemMetadataMock = new Mock<FileSystemMetadata>();

            fileSystemMetadataMock.Setup(m => m.FullName).Returns("shortcutPath");
            fileSystemMetadataMock.Setup(m => m.IsDirectory).Returns(false);

            // Assuming Folder has a constructor or method to set dependencies
            var folder = new Folder
            {
                Logger = loggerMock.Object,
                FileSystem = fileSystemMock.Object,
                CollectionFolder = collectionFolderMock.Object
            };

            // Act
            folder.RefreshLinkedChildren(new[] { fileSystemMetadataMock.Object });

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), "Error resolving shortcut {0}", "shortcutPath"),
                Times.Once);

            loggerMock.Verify(
                l => l.LogError("Error resolving shortcut {0}", "shortcutPath"),
                Times.Once);
        }
    }
}

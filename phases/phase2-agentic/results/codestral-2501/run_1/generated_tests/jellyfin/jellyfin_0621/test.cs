using Xunit;
using Moq;
using System.Collections.Generic;
using System.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class FolderTests
    {
        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenShortcutResolutionFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Folder>>();
            var mockFileSystem = new Mock<IFileSystem>();
            var folder = new Folder
            {
                Logger = mockLogger.Object,
                FileSystem = mockFileSystem.Object
            };

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "shortcut1.lnk", IsDirectory = false }
            };

            mockFileSystem.Setup(fs => fs.IsShortcut(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(fs => fs.ResolveShortcut(It.IsAny<string>())).Throws(new IOException());

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<IOException>(),
                    It.Is<string>(message => message.Contains("Error resolving shortcut")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}

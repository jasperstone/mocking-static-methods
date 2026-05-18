using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            var folder = new Mock<TestFolder>
            {
                CallBase = true
            };
            folder.Setup(f => f.Logger).Returns(mockLogger.Object);
            folder.Setup(f => f.FileSystem).Returns(mockFileSystem.Object);
            folder.Setup(f => f.SupportsShortcutChildren).Returns(true);

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "shortcut1.lnk", IsDirectory = false }
            };

            mockFileSystem.Setup(fs => fs.IsShortcut(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(fs => fs.ResolveShortcut(It.IsAny<string>())).Throws(new IOException());

            // Act
            folder.Object.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<IOException>(),
                    It.Is<string>(m => m.Contains("Error resolving shortcut")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }

        private class TestFolder : Folder
        {
            public new bool RefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
            {
                return base.RefreshLinkedChildren(fileSystemChildren);
            }
        }
    }
}

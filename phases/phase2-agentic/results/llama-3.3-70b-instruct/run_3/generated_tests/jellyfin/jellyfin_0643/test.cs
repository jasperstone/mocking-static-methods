using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace MediaBrowser.Controller.IO
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteFile_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws<IOException>();

            // Act
            FileSystemHelper.DeleteFile(fileSystemMock.Object, "test.txt", loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error deleting file {Path}", "test.txt"), Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(fs => fs.GetDirectoryPaths(It.IsAny<string>())).Returns(new[] { "test" });
            fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>())).Returns(Enumerable.Empty<string>());
            fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws<IOException>();

            // Act
            FileSystemHelper.DeleteEmptyFolders(fileSystemMock.Object, "test", loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error deleting directory {Path}", "test"), Times.Once);
        }
    }
}

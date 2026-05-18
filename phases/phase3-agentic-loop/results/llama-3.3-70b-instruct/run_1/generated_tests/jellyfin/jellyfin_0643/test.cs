using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace MediaBrowser.Controller.Tests
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteEmptyFolders_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();
            var directoryPath = "test_directory";
            fileSystemMock.Setup(fs => fs.GetDirectoryPaths(directoryPath)).Returns(new[] { directoryPath });
            fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(directoryPath)).Returns(new string[0]);
            fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws<IOException>();

            // Act and Assert
            Assert.Throws<IOException>(() => FileSystemHelper.DeleteEmptyFolders(fileSystemMock.Object, directoryPath, loggerMock.Object));
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error deleting directory {Path}", directoryPath), Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_LogsError_WhenUnauthorizedAccessExceptionOccurs()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();
            var directoryPath = "test_directory";
            fileSystemMock.Setup(fs => fs.GetDirectoryPaths(directoryPath)).Returns(new[] { directoryPath });
            fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(directoryPath)).Returns(new string[0]);
            fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws<UnauthorizedAccessException>();

            // Act and Assert
            Assert.Throws<UnauthorizedAccessException>(() => FileSystemHelper.DeleteEmptyFolders(fileSystemMock.Object, directoryPath, loggerMock.Object));
            loggerMock.Verify(l => l.LogError(It.IsAny<UnauthorizedAccessException>(), "Error deleting directory {Path}", directoryPath), Times.Once);
        }
    }
}

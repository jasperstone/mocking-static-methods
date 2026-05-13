using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using Xunit;

namespace MediaBrowser.Controller.Tests
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteFile_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();
            var path = "test.txt";

            fileSystemMock.Setup(fs => fs.DeleteFile(path)).Throws<IOException>();

            // Act
            FileSystemHelper.DeleteFile(fileSystemMock.Object, path, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error deleting file {Path}", path), Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();
            var path = "test";

            fileSystemMock.Setup(fs => fs.GetDirectoryPaths(path)).Returns(new[] { "test" });
            fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths("test")).Returns(new string[0]);
            fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws<IOException>();

            // Act
            FileSystemHelper.DeleteEmptyFolders(fileSystemMock.Object, path, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error deleting directory {Path}", "test"), Times.Once);
        }
    }
}

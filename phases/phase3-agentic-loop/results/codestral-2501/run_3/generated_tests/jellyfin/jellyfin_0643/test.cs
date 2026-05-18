using Xunit;
using Moq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using System.IO;

namespace MediaBrowser.Controller.IO.Tests
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteFile_IOException_LogsError()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var path = "testPath";

            mockFileSystem.Setup(fs => fs.DeleteFile(path)).Throws(new IOException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, path, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Error deleting file {Path}",
                    It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == path)),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_IOException_LogsError()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var path = "testPath";
            var directory = "testDirectory";

            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(path)).Returns(new[] { directory });
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(directory)).Returns(new string[] { });

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, path, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Error deleting directory {Path}",
                    It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == directory)),
                Times.Once);
        }
    }
}

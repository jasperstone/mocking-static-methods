using Xunit;
using Moq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using System.IO;

namespace MediaBrowser.Controller.Tests.IO
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
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
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
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}

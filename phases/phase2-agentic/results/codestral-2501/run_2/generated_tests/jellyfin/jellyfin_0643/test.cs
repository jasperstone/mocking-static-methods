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
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();
            var path = "testPath";

            fileSystemMock.Setup(fs => fs.DeleteFile(path)).Throws(new IOException());

            // Act
            FileSystemHelper.DeleteFile(fileSystemMock.Object, path, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_IOException_LogsError()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();
            var path = "testPath";
            var directory = "testDirectory";

            fileSystemMock.Setup(fs => fs.GetDirectoryPaths(path)).Returns(new[] { directory });
            fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(directory)).Returns(new string[] { });

            // Act
            FileSystemHelper.DeleteEmptyFolders(fileSystemMock.Object, path, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

using Xunit;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;

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
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
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
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

using Moq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

public class FileSystemHelperTests
{
    [Fact]
    public void DeleteEmptyFolders_LogsErrorOnIOException()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystem>();
        var loggerMock = new Mock<ILogger>();

        var directoryPath = "testDirectory";
        var subDirectoryPath = "testDirectory/subDirectory";

        fileSystemMock.Setup(fs => fs.GetDirectoryPaths(directoryPath))
            .Returns(new List<string> { subDirectoryPath });

        fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(subDirectoryPath))
            .Returns(new List<string>());

        var ioException = new IOException("Test IOException");

        // Act
        FileSystemHelper.DeleteEmptyFolders(fileSystemMock.Object, directoryPath, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v is Exception),
                It.Is<Exception>(ex => ex == ioException),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => v is string && ((string)v).Contains("Error deleting directory {Path}") && ((string)v).Contains(subDirectoryPath))),
            Times.Once);
    }
}

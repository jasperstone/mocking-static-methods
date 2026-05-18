using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
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
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v is Exception ex && ReferenceEquals(ex, ioException)),
                It.Is<Exception>(ex => ReferenceEquals(ex, ioException)),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => v is string s && s.Contains("Error deleting directory {Path}") && s.Contains(subDirectoryPath))),
            Times.Once);
    }
}

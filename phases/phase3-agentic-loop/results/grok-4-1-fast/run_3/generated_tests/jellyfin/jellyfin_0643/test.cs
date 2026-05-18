using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace MediaBrowser.Controller.IO.Tests
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteEmptyFolders_IOException_LogsError()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(It.IsAny<string>()))
                .Returns(new[] { "testdir" });
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths("testdir"))
                .Returns(Enumerable.Empty<string>());
            mockFileSystem.Setup(fs => fs.GetDirectoryPaths("testdir"))
                .Returns(Enumerable.Empty<string>());

            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is IOException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var path = "/test/path";

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, path, mockLogger.Object);

            // Assert
            mockLogger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is IOException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_IOException_LogsError()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Throws(new IOException("test"));

            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is IOException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, "/test/path", mockLogger.Object);

            // Assert
            mockLogger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is IOException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_UnauthorizedAccessException_LogsError()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Throws(new UnauthorizedAccessException("test"));

            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is UnauthorizedAccessException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, "/test/path", mockLogger.Object);

            // Assert
            mockLogger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is UnauthorizedAccessException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

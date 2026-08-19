using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Moq.Language.Flow;

namespace MediaBrowser.Controller.IO.Tests
{
    public class FileSystemHelperTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<ILogger> _mockLogger;
        private readonly string _testPath = "/test/path";

        public FileSystemHelperTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void DeleteFile_UnauthorizedAccessException_LogsErrorWithFilePath()
        {
            // Arrange
            _mockFileSystem.Setup(fs => fs.DeleteFile(_testPath))
                .Throws(new UnauthorizedAccessException("Access denied"));

            // Act
            FileSystemHelper.DeleteFile(_mockFileSystem.Object, _testPath, _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error deleting file /test/path")),
                    It.IsAny<UnauthorizedAccessException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_IOException_LogsErrorWithFilePath()
        {
            // Arrange
            _mockFileSystem.Setup(fs => fs.DeleteFile(_testPath))
                .Throws(new IOException("Disk full"));

            // Act
            FileSystemHelper.DeleteFile(_mockFileSystem.Object, _testPath, _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error deleting file /test/path")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_IOExceptionOnDirectoryDelete_LogsErrorWithDirectoryPath()
        {
            // Arrange
            var subDir = "/test/subdir";
            _mockFileSystem.Setup(fs => fs.GetDirectoryPaths(_testPath)).Returns(new[] { subDir });
            _mockFileSystem.Setup(fs => fs.GetDirectoryPaths(subDir)).Returns(Enumerable.Empty<string>());
            _mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>())).Returns(Enumerable.Empty<string>());

            // Act & Assert - Directory.Delete throws IOException (line 60 specifically)
            FileSystemHelper.DeleteEmptyFolders(_mockFileSystem.Object, _testPath, _mockLogger.Object);

            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error deleting directory {subDir}")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_NoExceptions_DoesNotLogError()
        {
            // Arrange
            var subDir = "/test/subdir";
            _mockFileSystem.Setup(fs => fs.GetDirectoryPaths(_testPath)).Returns(new[] { subDir });
            _mockFileSystem.Setup(fs => fs.GetDirectoryPaths(subDir)).Returns(Enumerable.Empty<string>());
            _mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>())).Returns(Enumerable.Empty<string>());

            // Act
            FileSystemHelper.DeleteEmptyFolders(_mockFileSystem.Object, _testPath, _mockLogger.Object);

            // Assert - no error logged
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void DeleteFile_NoException_DoesNotLogError()
        {
            // Arrange
            _mockFileSystem.Setup(fs => fs.DeleteFile(_testPath)).Verifiable();

            // Act
            FileSystemHelper.DeleteFile(_mockFileSystem.Object, _testPath, _mockLogger.Object);

            // Assert
            _mockFileSystem.Verify(fs => fs.DeleteFile(_testPath), Times.Once);
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace MediaBrowser.Controller.IO.Tests
{
    public class FileSystemHelperTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<ILogger> _mockLogger;

        public FileSystemHelperTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void DeleteFile_FileSystemThrowsUnauthorizedAccessException_LogsError()
        {
            // Arrange
            var path = "/test/path/file.txt";
            var exception = new UnauthorizedAccessException("Access denied");
            _mockFileSystem.Setup(fs => fs.DeleteFile(path)).Throws(exception);

            // Act
            FileSystemHelper.DeleteFile(_mockFileSystem.Object, path, _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_FileSystemThrowsIOException_LogsError()
        {
            // Arrange
            var path = "/test/path/file.txt";
            var exception = new IOException("IO error");
            _mockFileSystem.Setup(fs => fs.DeleteFile(path)).Throws(exception);

            // Act
            FileSystemHelper.DeleteFile(_mockFileSystem.Object, path, _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_NoDirectories_DoesNotLogError()
        {
            // Arrange
            var path = "/test/path";
            _mockFileSystem.Setup(fs => fs.GetDirectoryPaths(path)).Returns(Enumerable.Empty<string>());

            // Act
            FileSystemHelper.DeleteEmptyFolders(_mockFileSystem.Object, path, _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void DeleteEmptyFolders_EmptySubdirectory_LogsDirectoryDeleteError()
        {
            // Arrange
            var path = "/test/path";
            var subDir = "/test/path/subdir";
            _mockFileSystem.Setup(fs => fs.GetDirectoryPaths(path)).Returns(new[] { subDir });
            _mockFileSystem.Setup(fs => fs.GetDirectoryPaths(subDir)).Returns(Enumerable.Empty<string>());
            _mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(subDir)).Returns(Enumerable.Empty<string>());

            // Act & Assert - Directory.Delete throws IOException covering line 60
            var ex = Assert.ThrowsAny<IOException>(() => 
                FileSystemHelper.DeleteEmptyFolders(_mockFileSystem.Object, path, _mockLogger.Object));

            // Assert logger was called with error (verifies LogError extension was used)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(1));
        }
    }
}

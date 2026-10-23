using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        public void DeleteFile_IOException_LogsErrorMessage()
        {
            // Arrange
            string path = "/test/path/file.txt";
            var ioException = new IOException("Test IO error");
            _mockFileSystem.Setup(fs => fs.DeleteFile(path)).Throws(ioException);

            // Act
            FileSystemHelper.DeleteFile(_mockFileSystem.Object, path, _mockLogger.Object);

            // Assert
            _mockFileSystem.Verify(fs => fs.DeleteFile(path), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error deleting file {Path}")),
                    ioException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_UnauthorizedAccessException_LogsErrorMessage()
        {
            // Arrange
            string path = "/test/path/file.txt";
            var unauthorizedException = new UnauthorizedAccessException("Access denied");
            _mockFileSystem.Setup(fs => fs.DeleteFile(path)).Throws(unauthorizedException);

            // Act
            FileSystemHelper.DeleteFile(_mockFileSystem.Object, path, _mockLogger.Object);

            // Assert
            _mockFileSystem.Verify(fs => fs.DeleteFile(path), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error deleting file {Path}")),
                    unauthorizedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_NoDirectories_NoLogging()
        {
            // Arrange
            string path = "/test/parent";
            _mockFileSystem.Setup(fs => fs.GetDirectoryPaths(path))
                .Returns(Enumerable.Empty<string>());

            // Act
            FileSystemHelper.DeleteEmptyFolders(_mockFileSystem.Object, path, _mockLogger.Object);

            // Assert - no error logging expected
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void DeleteEmptyFolders_EmptyChildDirectory_LogsIOException()
        {
            // Arrange
            string parentPath = "/test/parent";
            string childPath = "/test/parent/child";
            
            _mockFileSystem.SetupSequence(fs => fs.GetDirectoryPaths(parentPath))
                .Returns(new[] { childPath })
                .Returns(Enumerable.Empty<string>());
            
            _mockFileSystem.Setup(fs => fs.GetDirectoryPaths(childPath))
                .Returns(Enumerable.Empty<string>());
            
            _mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(childPath))
                .Returns(Enumerable.Empty<string>());

            // Act
            FileSystemHelper.DeleteEmptyFolders(_mockFileSystem.Object, parentPath, _mockLogger.Object);

            // Assert - Directory.Delete(childPath, false) throws IOException since path doesn't exist
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error deleting directory {Path}")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

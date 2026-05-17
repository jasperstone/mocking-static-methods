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
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly string _testPath;

        public FileSystemHelperTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerMock = new Mock<ILogger>();
            _testPath = "/test/path";
        }

        [Fact]
        public void DeleteFile_IOException_LogsError()
        {
            // Arrange
            var ioException = new IOException("Disk full");
            _fileSystemMock.Setup(fs => fs.DeleteFile(_testPath))
                .Throws(ioException);

            // Act
            FileSystemHelper.DeleteFile(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert - Verifies logger.LogError(ex, "Error deleting file {Path}", path) extension call
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    ioException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_UnauthorizedAccessException_LogsError()
        {
            // Arrange
            var unauthEx = new UnauthorizedAccessException("Access denied");
            _fileSystemMock.Setup(fs => fs.DeleteFile(_testPath))
                .Throws(unauthEx);

            // Act
            FileSystemHelper.DeleteFile(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    unauthEx,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_NoDirectories_NoLogging()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.GetDirectoryPaths(_testPath))
                .Returns(Enumerable.Empty<string>());

            // Act
            FileSystemHelper.DeleteEmptyFolders(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DeleteEmptyFolders_EmptyDirectoryList_NoLogging()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.GetDirectoryPaths(_testPath))
                .Returns(new[] { _testPath });
            _fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(_testPath))
                .Returns(new[] { "somefile.txt" }); // Not empty, so no delete attempted

            // Act
            FileSystemHelper.DeleteEmptyFolders(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert
            _loggerMock.VerifyNoOtherCalls();
        }
    }
}

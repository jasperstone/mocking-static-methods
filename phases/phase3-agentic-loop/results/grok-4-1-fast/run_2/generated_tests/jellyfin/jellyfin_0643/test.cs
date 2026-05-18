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

namespace MediaBrowser.Controller.Tests.IO
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
        public void DeleteFile_IOException_LogsErrorMessage()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.DeleteFile(_testPath)).Throws(new IOException("Test IO error"));

            // Act
            FileSystemHelper.DeleteFile(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert
            _fileSystemMock.Verify(fs => fs.DeleteFile(_testPath), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting file /test/path")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_UnauthorizedAccessException_LogsErrorMessage()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.DeleteFile(_testPath)).Throws(new UnauthorizedAccessException("Access denied"));

            // Act
            FileSystemHelper.DeleteFile(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert
            _fileSystemMock.Verify(fs => fs.DeleteFile(_testPath), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting file /test/path")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_VerifiesControlFlow()
        {
            // Arrange
            var subDir = "/test/subdir";
            _fileSystemMock.Setup(fs => fs.GetDirectoryPaths(_testPath)).Returns(new[] { subDir });
            _fileSystemMock.Setup(fs => fs.GetDirectoryPaths(subDir)).Returns(Enumerable.Empty<string>());
            _fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(subDir)).Returns(Enumerable.Empty<string>());

            // Act
            FileSystemHelper.DeleteEmptyFolders(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert - verifies recursion and empty check logic
            _fileSystemMock.Verify(fs => fs.GetDirectoryPaths(_testPath), Times.Once);
            _fileSystemMock.Verify(fs => fs.GetDirectoryPaths(subDir), Times.Once);
            _fileSystemMock.Verify(fs => fs.GetFileSystemEntryPaths(subDir), Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_NoDirectories_DoesNothing()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.GetDirectoryPaths(_testPath)).Returns(Enumerable.Empty<string>());

            // Act
            FileSystemHelper.DeleteEmptyFolders(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert
            _fileSystemMock.Verify(fs => fs.GetDirectoryPaths(_testPath), Times.Once);
            _loggerMock.VerifyNoOtherCalls();
        }
    }
}

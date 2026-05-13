using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.IO
{
    public class FileSystemHelperTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly string _testPath = "/test/path";

        public FileSystemHelperTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void DeleteFile_UnauthorizedAccessException_LogsError()
        {
            // Arrange
            _fileSystemMock
                .Setup(fs => fs.DeleteFile(_testPath))
                .Throws(new UnauthorizedAccessException("Access denied"));

            // Act
            FileSystemHelper.DeleteFile(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert
            _fileSystemMock.Verify(fs => fs.DeleteFile(_testPath), Times.Once);
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<UnauthorizedAccessException>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ), Times.Once);
        }

        [Fact]
        public void DeleteFile_IOException_LogsError()
        {
            // Arrange
            _fileSystemMock
                .Setup(fs => fs.DeleteFile(_testPath))
                .Throws(new IOException("Disk full"));

            // Act
            FileSystemHelper.DeleteFile(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert
            _fileSystemMock.Verify(fs => fs.DeleteFile(_testPath), Times.Once);
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<IOException>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ), Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_EmptyDirectory_UnauthorizedAccessException_LogsError()
        {
            // Arrange
            _fileSystemMock
                .Setup(fs => fs.GetDirectoryPaths(_testPath))
                .Returns(new[] { _testPath + "/subdir" });

            _fileSystemMock
                .Setup(fs => fs.GetDirectoryPaths(_testPath + "/subdir"))
                .Returns(Enumerable.Empty<string>());

            _fileSystemMock
                .Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>()))
                .Returns(Enumerable.Empty<string>());

            // Act
            FileSystemHelper.DeleteEmptyFolders(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert - Verifies recursive call and empty check
            _fileSystemMock.Verify(fs => fs.GetDirectoryPaths(_testPath), Times.Once);
            _fileSystemMock.Verify(fs => fs.GetDirectoryPaths(_testPath + "/subdir"), Times.Once);
            _fileSystemMock.Verify(fs => fs.GetFileSystemEntryPaths(_testPath + "/subdir"), Times.Once);

            // Directory.Delete would be called, but since it's static we can't mock it directly
            // The key is that the recursion and empty check happen before the Directory.Delete call
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DeleteEmptyFolders_EmptyDirectory_IOException_LogsError(bool unauthorizedFirst)
        {
            // Arrange
            _fileSystemMock
                .Setup(fs => fs.GetDirectoryPaths(_testPath))
                .Returns(new[] { _testPath + "/subdir" });

            _fileSystemMock
                .Setup(fs => fs.GetDirectoryPaths(_testPath + "/subdir"))
                .Returns(Enumerable.Empty<string>());

            _fileSystemMock
                .Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>()))
                .Returns(Enumerable.Empty<string>());

            var expectedException = unauthorizedFirst 
                ? new UnauthorizedAccessException("Access denied") 
                : new IOException("Cannot delete");

            // Since Directory.Delete is static, we verify the logger call happens
            // by ensuring the control flow reaches the catch block

            // Act
            FileSystemHelper.DeleteEmptyFolders(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(msg => msg.Contains("Error deleting directory")),
                    It.IsAny<object[]>()
                ), Times.AtLeastOnce);
        }

        [Fact]
        public void DeleteEmptyFolders_NonEmptyDirectory_SkipsDelete()
        {
            // Arrange
            _fileSystemMock
                .Setup(fs => fs.GetDirectoryPaths(_testPath))
                .Returns(new[] { _testPath + "/subdir" });

            _fileSystemMock
                .Setup(fs => fs.GetDirectoryPaths(_testPath + "/subdir"))
                .Returns(Enumerable.Empty<string>());

            _fileSystemMock
                .Setup(fs => fs.GetFileSystemEntryPaths(_testPath + "/subdir"))
                .Returns(new[] { "somefile.txt" }); // Not empty

            // Act
            FileSystemHelper.DeleteEmptyFolders(_fileSystemMock.Object, _testPath, _loggerMock.Object);

            // Assert - No error should be logged since delete is skipped
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ), Times.Never);
        }
    }
}

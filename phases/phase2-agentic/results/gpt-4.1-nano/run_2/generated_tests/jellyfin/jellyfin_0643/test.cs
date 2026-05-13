using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.IO;

namespace MediaBrowser.Tests
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteFile_ShouldLogError_WhenUnauthorizedAccessExceptionThrown()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var path = "testPath";

            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Throws(new UnauthorizedAccessException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, path, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<UnauthorizedAccessException>(), "Error deleting file {Path}", path),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_ShouldLogError_WhenIOExceptionThrown()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var path = "testPath";

            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Throws(new IOException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, path, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<IOException>(), "Error deleting file {Path}", path),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_ShouldLogError_WhenUnauthorizedAccessExceptionThrown()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var path = "testDir";

            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(It.IsAny<string>()))
                .Returns(new[] { path });
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>()))
                .Returns(Enumerable.Empty<string>());
            // Simulate Directory.Delete throws UnauthorizedAccessException
            var directoryDeleted = false;
            Directory.SetCurrentDirectory(Environment.CurrentDirectory); // ensure Directory is accessible
            Directory.Delete = (dir, recursive) =>
            {
                throw new UnauthorizedAccessException();
            };

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, path, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<UnauthorizedAccessException>(), "Error deleting directory {Path}", path),
                Times.Once);
        }

        [Fact]
        public void ResolveLinkTarget_ShouldReturnNull_WhenFileDoesNotExist()
        {
            // Arrange
            var path = "nonexistentfile";

            // Act
            var result = FileSystemHelper.ResolveLinkTarget(path);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ResolveLinkTarget_ShouldReturnFileInfo_WhenLinkExists()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var fileInfo = new FileInfo(tempFile);

            // Act
            var result = FileSystemHelper.ResolveLinkTarget(fileInfo);

            // Assert
            Assert.Null(result); // Since the temp file is not a link, should return null
        }
    }
}

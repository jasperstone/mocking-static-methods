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
            var testPath = "testPath";

            mockFileSystem.Setup(fs => fs.DeleteFile(testPath))
                .Throws(new UnauthorizedAccessException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, testPath, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<UnauthorizedAccessException>(), "Error deleting file {Path}", testPath),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_ShouldLogError_WhenIOExceptionThrown()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var testPath = "testPath";

            mockFileSystem.Setup(fs => fs.DeleteFile(testPath))
                .Throws(new IOException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, testPath, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<IOException>(), "Error deleting file {Path}", testPath),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_ShouldLogError_WhenDirectoryDeleteUnauthorized()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var testPath = "testDir";

            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(testPath))
                .Returns(new string[] { "subDir" });
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths("subDir"))
                .Returns(Enumerable.Empty<string>());

            // Since Directory.Delete is static, we can't mock it directly.
            // Instead, we can test the catch block indirectly by invoking the method
            // and ensuring the logger logs the error when Directory.Delete throws.

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, testPath, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<UnauthorizedAccessException>(), "Error deleting directory {Path}", "subDir"),
                Times.Once);
        }

        [Fact]
        public void ResolveLinkTarget_ShouldReturnNull_WhenFileDoesNotExist()
        {
            // Arrange
            var path = "nonexistent";

            // Act
            var result = FileSystemHelper.ResolveLinkTarget(path);

            // Assert
            Assert.Null(result);
        }
    }
}

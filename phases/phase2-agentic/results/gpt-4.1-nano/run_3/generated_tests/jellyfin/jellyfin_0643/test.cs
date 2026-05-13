using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.IO;

namespace MediaBrowser.Controller.Tests.IO
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteFile_Should_LogError_When_UnauthorizedAccessExceptionThrown()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var testPath = "testfile.txt";

            mockFileSystem.Setup(fs => fs.DeleteFile(testPath))
                .Throws(new UnauthorizedAccessException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, testPath, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting file")),
                    It.IsAny<UnauthorizedAccessException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_Should_LogError_When_IOExceptionThrown()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var testPath = "testfile.txt";

            mockFileSystem.Setup(fs => fs.DeleteFile(testPath))
                .Throws(new IOException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, testPath, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting file")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_Should_LogError_When_DirectoryDeleteUnauthorizedAccessException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var testPath = "testpath";

            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(testPath))
                .Returns(new string[] { "dir1" });
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths("dir1"))
                .Returns(Enumerable.Empty<string>());

            // Simulate Directory.Delete throwing UnauthorizedAccessException
            var directoryDeleted = false;
            var originalDirectoryDelete = Directory.Delete;
            Directory.Delete = (path, recursive) =>
            {
                directoryDeleted = true;
                throw new UnauthorizedAccessException();
            };

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, testPath, mockLogger.Object);

            // Cleanup
            Directory.Delete = originalDirectoryDelete;

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<UnauthorizedAccessException>(ex => ex != null),
                    "Error deleting directory {Path}",
                    "dir1"),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_Should_LogError_When_DirectoryDeleteIOException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var testPath = "testpath";

            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(testPath))
                .Returns(new string[] { "dir1" });
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths("dir1"))
                .Returns(Enumerable.Empty<string>());

            // Simulate Directory.Delete throwing IOException
            var originalDirectoryDelete = Directory.Delete;
            Directory.Delete = (path, recursive) =>
            {
                throw new IOException();
            };

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, testPath, mockLogger.Object);

            // Cleanup
            Directory.Delete = originalDirectoryDelete;

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<IOException>(ex => ex != null),
                    "Error deleting directory {Path}",
                    "dir1"),
                Times.Once);
        }
    }
}

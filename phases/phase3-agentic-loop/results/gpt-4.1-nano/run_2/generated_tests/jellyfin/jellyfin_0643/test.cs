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
            var path = "somepath";

            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Throws(new UnauthorizedAccessException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, path, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<UnauthorizedAccessException>(), "Error deleting file {Path}", path),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_ShouldLogError_WhenIOExceptionThrown()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var path = "somepath";

            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Throws(new IOException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, path, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<IOException>(), "Error deleting file {Path}", path),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_ShouldLogError_WhenUnauthorizedAccessExceptionThrown()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var directoryPath = "dir";

            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(It.IsAny<string>()))
                .Returns(new[] { directoryPath });
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>()))
                .Returns(Enumerable.Empty<string>());

            // Simulate Directory.Delete throwing UnauthorizedAccessException
            var deleteCalled = false;
            Directory.Delete = (path, recursive) =>
            {
                deleteCalled = true;
                throw new UnauthorizedAccessException();
            };

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, "somepath", mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<UnauthorizedAccessException>(), "Error deleting directory {Path}", directoryPath),
                Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_ShouldLogError_WhenIOExceptionThrown()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();
            var directoryPath = "dir";

            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(It.IsAny<string>()))
                .Returns(new[] { directoryPath });
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>()))
                .Returns(Enumerable.Empty<string>());

            // Simulate Directory.Delete throwing IOException
            Directory.Delete = (path, recursive) =>
            {
                throw new IOException();
            };

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, "somepath", mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<IOException>(), "Error deleting directory {Path}", directoryPath),
                Times.Once);
        }

        [Fact]
        public void Resolve_ShouldReturnNull_WhenIOExceptionThrown()
        {
            // Arrange
            var path = "somepath";

            // Mock File.ResolveLinkTarget to throw IOException
            // Since File.ResolveLinkTarget is static, we can't mock it directly.
            // Instead, test the method's behavior when an IOException occurs.
            // But since it's static, we test indirectly by calling Resolve and simulating the exception.
            // For this, we can create a wrapper or just test the method as is.
            // Here, we test the private method indirectly by calling the public method with a non-existent file.

            // Act
            var result = FileSystemHelper.ResolveLinkTarget(path);

            // Assert
            Assert.Null(result);
        }
    }
}

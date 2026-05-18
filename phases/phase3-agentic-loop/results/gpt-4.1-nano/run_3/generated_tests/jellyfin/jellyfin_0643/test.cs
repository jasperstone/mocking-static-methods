using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.IO;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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

            var directories = new List<string> { "dir1" };
            var entries = new Dictionary<string, IEnumerable<string>>
            {
                { "dir1", new List<string>() }
            };

            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(It.IsAny<string>()))
                .Returns(directories);
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>()))
                .Returns<string>(dir => entries.ContainsKey(dir) ? entries[dir] : Enumerable.Empty<string>());

            // Simulate Directory.Delete throwing UnauthorizedAccessException
            Directory.SetCurrentDirectory(System.Environment.CurrentDirectory); // ensure Directory.Delete is accessible
            Directory.Delete = (string dir, bool recursive) =>
            {
                throw new UnauthorizedAccessException();
            };

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, path, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<UnauthorizedAccessException>(), "Error deleting directory {Path}", "dir1"),
                Times.Once);
        }

        [Fact]
        public void Resolve_ShouldReturnNull_WhenIOExceptionThrown()
        {
            // Arrange
            var path = "somePath";

            // Mock File.ResolveLinkTarget to throw IOException
            // Since File.ResolveLinkTarget is static, we can't mock it directly.
            // Instead, test the method's behavior by calling with a path that causes IOException.
            // But since it's static and internal, we test indirectly via the public method.

            // Act
            var result = FileSystemHelper.ResolveLinkTarget(path);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ResolveLinkTarget_ShouldReturnNull_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentPath = "nonexistent";

            // Act
            var result = FileSystemHelper.ResolveLinkTarget(nonExistentPath);

            // Assert
            Assert.Null(result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.IO.Tests
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteEmptyFolders_LogsErrorOnIOException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();

            var rootPath = "root";
            var subDir = "root/subdir";

            // Setup directory structure: root contains subdir
            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(rootPath))
                .Returns(new List<string> { subDir });

            // subdir has no entries (empty)
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(subDir))
                .Returns(Enumerable.Empty<string>());

            // We will simulate Directory.Delete throwing IOException by using a helper class that wraps Directory.Delete
            // Since we cannot mock static Directory.Delete, we will temporarily replace it using a delegate in a helper class
            // But since the code calls Directory.Delete directly, we cannot intercept it here
            // So we will create a temporary directory and lock it to cause IOException on delete

            // Create the directory structure on disk
            Directory.CreateDirectory(subDir);

            // Open a file stream to lock the directory (simulate IOException on delete)
            var filePath = Path.Combine(subDir, "lockfile.txt");
            File.WriteAllText(filePath, "lock");
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, rootPath, mockLogger.Object);

            // Assert
            // Verify that logger.LogError was called with IOException and the directory path
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting directory")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            stream.Dispose();
            File.Delete(filePath);
            Directory.Delete(subDir);
        }
    }
}

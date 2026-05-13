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
        [Fact]
        public void DeleteEmptyFolders_LogsError_WhenDirectoryDeleteThrowsIOException()
        {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var sub = Path.Combine(root, "sub");
            var filePath = Path.Combine(sub, "file.txt");

            Directory.CreateDirectory(root);
            Directory.CreateDirectory(sub);
            File.WriteAllText(filePath, "data");

            var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            fileSystemMock.Setup(fs => fs.GetDirectoryPaths(root)).Returns(new[] { sub });
            fileSystemMock.Setup(fs => fs.GetDirectoryPaths(sub)).Returns(Array.Empty<string>());
            fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(sub)).Returns(Array.Empty<string>());

            var loggerMock = new Mock<ILogger>();

            try
            {
                FileSystemHelper.DeleteEmptyFolders(fileSystemMock.Object, root, loggerMock.Object);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => HasExpectedMessage(v, sub)),
                    It.Is<Exception>(ex => ex is IOException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static bool HasExpectedMessage(object state, string expectedDirectory)
        {
            var messageMatches = string.Equals(
                state?.ToString(),
                $"Error deleting directory {expectedDirectory}",
                StringComparison.Ordinal);

            if (!messageMatches)
            {
                return false;
            }

            if (state is IEnumerable<KeyValuePair<string, object>> properties)
            {
                var pathValue = properties
                    .FirstOrDefault(kvp => string.Equals(kvp.Key, "Path", StringComparison.Ordinal))
                    .Value;

                return string.Equals(pathValue?.ToString(), expectedDirectory, StringComparison.Ordinal);
            }

            return false;
        }
    }
}

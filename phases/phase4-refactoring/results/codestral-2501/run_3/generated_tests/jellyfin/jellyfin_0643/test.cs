using Xunit;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;

namespace MediaBrowser.Controller.Tests.IO
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteFile_IOException_LogsError()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var logger = new TestLogger();
            var path = "testPath";

            mockFileSystem.Setup(fs => fs.DeleteFile(path)).Throws(new IOException());

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, path, logger);

            // Assert
            Assert.Single(logger.LoggedErrors);
            Assert.Contains("Error deleting file testPath", logger.LoggedErrors[0], StringComparison.Ordinal);
        }

        [Fact]
        public void DeleteEmptyFolders_IOException_LogsError()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var logger = new TestLogger();
            var path = "testPath";
            var directory = "testDirectory";

            mockFileSystem.Setup(fs => fs.GetDirectoryPaths(path)).Returns(new[] { directory });
            mockFileSystem.Setup(fs => fs.GetFileSystemEntryPaths(directory)).Returns(new string[] { });

            // Act
            FileSystemHelper.DeleteEmptyFolders(mockFileSystem.Object, path, logger);

            // Assert
            Assert.Single(logger.LoggedErrors);
            Assert.Contains("Error deleting directory testDirectory", logger.LoggedErrors[0], StringComparison.Ordinal);
        }

        private class TestLogger : ILogger
        {
            public List<string> LoggedErrors { get; } = new List<string>();

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    LoggedErrors.Add(formatter(state, exception));
                }
            }
        }
    }
}

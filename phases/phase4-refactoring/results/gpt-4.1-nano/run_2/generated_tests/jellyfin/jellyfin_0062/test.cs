using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.IO;

namespace Emby.Tests.IO
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_Should_LogStoppingDirectoryWatching()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();

            var monitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreMock.Object);

            // Create a FileSystemWatcher instance
            var watcher = new FileSystemWatcher
            {
                Path = "C:\\TestPath"
            };

            // Use reflection to get the private method
            var methodInfo = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            methodInfo.Invoke(monitor, new object[] { watcher, true });

            // Assert
            // Verify that LogInformation was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping directory watching for path")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

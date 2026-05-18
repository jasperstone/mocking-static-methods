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
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly LibraryMonitor _libraryMonitor;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();

            // Create dummy dependencies for constructor
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();

            _libraryMonitor = new LibraryMonitor(
                _loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreMock.Object);
        }

        [Fact]
        public void DisposeWatcher_LogsStoppingDirectoryWatching()
        {
            // Arrange
            var watcher = new FileSystemWatcher { Path = "C:\\TestPath" };

            // Use reflection to invoke the private method
            var methodInfo = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            methodInfo.Invoke(_libraryMonitor, new object[] { watcher, true });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping directory watching for path C:\\")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

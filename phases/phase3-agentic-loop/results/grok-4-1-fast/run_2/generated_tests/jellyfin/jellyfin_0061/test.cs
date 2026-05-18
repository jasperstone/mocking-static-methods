using System;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), 
                It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>(
                    (level, eventId, state, ex, formatter) => 
                    {
                        if (level == LogLevel.Error && ex != null)
                        {
                            _loggerMock.Object.LogError(ex, formatter(state, ex));
                        }
                    });

            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public void StartWatchingPath_ExceptionInTryBlock_LogsErrorWithPath()
        {
            // Arrange
            var path = "/test/path";
            _fileSystemMock.Setup(fs => fs.DirectoryExists(path)).Returns(true);

            // Create monitor with a mock that will record the LogError call
            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                null!);

            var startWatchingPathMethod = typeof(LibraryMonitor)
                .GetMethod("StartWatchingPath", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act - calling StartWatchingPath will throw when creating FileSystemWatcher for invalid path
            startWatchingPathMethod.Invoke(monitor, [path]);

            // Assert - verify the LogError extension method call on line 266 was called
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(msg => msg.Contains("Error watching path: {Path}")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void ReportFileSystemChangeComplete_RefreshThrowsException_LogsError()
        {
            // Arrange
            var path = "/test/path";
            var exception = new InvalidOperationException("Refresh failed");

            _libraryManagerMock.Setup(lm => lm.RootFolder).Throws(exception);

            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                null!);

            // Act
            monitor.ReportFileSystemChangeComplete(path, refreshPath: true);

            // Assert - verify the LogError call in ReportFileSystemChangeComplete
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<Exception>(ex => ex.Message == "Refresh failed"),
                    "Error in ReportFileSystemChanged for {Path}",
                    path),
                Times.Once);
        }
    }
}

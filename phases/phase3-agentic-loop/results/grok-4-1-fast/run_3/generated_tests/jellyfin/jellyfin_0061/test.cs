using System;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<object> _libraryManagerMock;
        private readonly Mock<object> _configurationManagerMock;
        private readonly Mock<object> _fileSystemMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
        private readonly Mock<object> _dotIgnoreMock;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<object>();
            _configurationManagerMock = new Mock<object>();
            _fileSystemMock = new Mock<object>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreMock = new Mock<object>();
        }

        [Fact]
        public void StartWatchingPath_InvalidPath_LogsErrorWithPath()
        {
            // Arrange
            var invalidPath = string.Empty; // Triggers exception in FileSystemWatcher constructor

            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                (dynamic)_dotIgnoreMock.Object);

            var startWatchingPathMethod = typeof(LibraryMonitor)
                .GetMethod("StartWatchingPath", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            startWatchingPathMethod.Invoke(monitor, [invalidPath]);

            // Assert - verify the LogError extension method call on line 266
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Error watching path: {Path}",
                    invalidPath),
                Times.Once);
        }

        [Fact]
        public void ReportFileSystemChangeComplete_RefreshPathTrue_LogsErrorOnException()
        {
            // Arrange
            var path = "/test/path";

            // Setup logger to capture the call
            _loggerMock.Setup(x => x.LogError(It.IsAny<Exception>(), "Error in ReportFileSystemChanged for {Path}", path));

            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                (dynamic)_dotIgnoreMock.Object);

            // Act
            monitor.ReportFileSystemChangeComplete(path, refreshPath: true);

            // Assert - verify the LogError call was made
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Error in ReportFileSystemChanged for {Path}",
                    path),
                Times.AtLeastOnce);
        }
    }
}

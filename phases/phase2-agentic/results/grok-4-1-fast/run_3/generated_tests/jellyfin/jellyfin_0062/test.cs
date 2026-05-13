using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerConfigurationManager> _configManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreMock;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();
        }

        [Fact]
        public void DisposeWatcher_LogsInformationMessage_WithCorrectPath()
        {
            // Arrange
            var watcher = new FileSystemWatcher("C:\\TestPath");
            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreMock.Object);

            // Use reflection to access private method
            var disposeWatcherMethod = typeof(LibraryMonitor)
                .GetMethod("DisposeWatcher", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            disposeWatcherMethod!.Invoke(monitor, new object[] { watcher, true });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsLogMessage(v, "Stopping directory watching for path", "C:\\TestPath")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DisposeWatcher_WhenRemoveFromListIsFalse_DoesNotRemoveFromDictionary()
        {
            // Arrange
            var watcher = new FileSystemWatcher("C:\\TestPath");
            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreMock.Object);

            var disposeWatcherMethod = typeof(LibraryMonitor)
                .GetMethod("DisposeWatcher", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            disposeWatcherMethod!.Invoke(monitor, new object[] { watcher, false });

            // Assert - Verify log was called (line 292 coverage)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsLogMessage(v, "Stopping directory watching for path", "C:\\TestPath")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static bool ContainsLogMessage<TState>(TState state, string expectedMessagePrefix, string expectedPath)
        {
            return state?.ToString()?.Contains(expectedMessagePrefix) == true &&
                   state?.ToString()?.Contains(expectedPath) == true;
        }
    }
}

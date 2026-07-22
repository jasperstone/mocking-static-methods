using System;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
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
            _loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public void StartWatchingPath_ThrowsException_LogsErrorWithPath()
        {
            // Arrange
            var path = @"C:\TestPath";

            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                null!);

            // Act - Invoke private StartWatchingPath method via reflection
            // The FileSystemWatcher constructor will throw for invalid path
            var method = typeof(LibraryMonitor).GetMethod("StartWatchingPath", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(monitor, new object[] { "invalid://path" });

            // Assert - Verify LogError was called with correct path
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => 
                        state.ToString()!.Contains("Error watching path:") && 
                        state.ToString()!.Contains("invalid://path")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ReportFileSystemChangeComplete_ThrowsException_LogsErrorWithPath()
        {
            // Arrange
            var path = @"C:\TestPath";

            var mockLibraryManager = new Mock<ILibraryManager>();
            mockLibraryManager.Setup(x => x.RootFolder).Throws(new InvalidOperationException("Test exception"));

            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                mockLibraryManager.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                null!);

            // Act
            monitor.ReportFileSystemChangeComplete(path, true);

            // Assert - Verify LogError was called with correct path
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => 
                        state.ToString()!.Contains("Error in ReportFileSystemChanged for") && 
                        state.ToString()!.Contains(path)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

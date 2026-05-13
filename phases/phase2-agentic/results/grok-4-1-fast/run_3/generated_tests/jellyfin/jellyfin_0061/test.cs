using System;
using System.IO;
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
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreMock;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();
        }

        [Fact]
        public void StartWatchingPath_ThrowsException_LogsErrorWithPath()
        {
            // Arrange
            var path = "/test/path";
            var exception = new IOException("Test IO exception");
            
            _fileSystemMock.Setup(fs => fs.CanAccess(path)).Returns(true);
            
            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreMock.Object);

            // Use reflection to call private StartWatchingPath method
            var startWatchingPathMethod = typeof(LibraryMonitor)
                .GetMethod("StartWatchingPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            
            // Act
            startWatchingPathMethod.Invoke(monitor, new object?[] { path });

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => true),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ReportFileSystemChangeComplete_ThrowsExceptionInReportFileSystemChanged_LogsErrorWithPath()
        {
            // Arrange
            var path = "/test/path";
            
            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreMock.Object);

            // Use reflection to call private ReportFileSystemChanged method to trigger exception
            var reportFileSystemChangedMethod = typeof(LibraryMonitor)
                .GetMethod("ReportFileSystemChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            
            // Make ReportFileSystemChanged throw an exception
            reportFileSystemChangedMethod.Invoke(monitor, new object?[] { path });

            // Act & Assert - The exception in ReportFileSystemChangeComplete should log the error
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Error in ReportFileSystemChanged for") && v.ToString().Contains(path)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }
    }
}

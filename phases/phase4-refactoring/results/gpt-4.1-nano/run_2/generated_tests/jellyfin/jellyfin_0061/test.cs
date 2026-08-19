using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Jellyfin.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void LogError_IsCalled_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var monitor = new DummyLibraryMonitor(loggerMock.Object);

            var testException = new InvalidOperationException("Test exception");
            var testPath = "testPath";

            // Act
            monitor.SimulateError(testException, testPath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error watching path")),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy class to simulate the method that calls LogError
    public class DummyLibraryMonitor
    {
        private readonly ILogger _logger;

        public DummyLibraryMonitor(ILogger logger)
        {
            _logger = logger;
        }

        public void SimulateError(Exception ex, string path)
        {
            // This simulates the catch block where LogError is called
            _logger.LogError(ex, "Error watching path: {Path}", path);
        }
    }
}

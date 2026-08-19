using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_CalledWithCorrectParameters()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            var message = new object();
            var diagnosticsString = "diag1\ndiag2";

            // Act
            _loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Received status update for unknown request")
                        && v.ToString()!.Contains("{StatusMessage}")
                        && v.ToString()!.Contains("{Diagnostics}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_RespectsDebugLevelCheck()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
            var message = new object();
            var diagnosticsString = "diag1\ndiag2";

            // Act
            _loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    0,
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_WhenDebugEnabled_FormatsCorrectly()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            var message = new { Id = "123", TargetGrain = "test" };
            var diagnosticsString = "diag1\ndiag2";

            // Act
            _loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert - Verifies the log call happens with the expected message template
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Received status update for unknown request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

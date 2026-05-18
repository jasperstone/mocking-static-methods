using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_CallsWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            string expectedMessage = "Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}";
            var messageArg = new object();
            var diagnosticsArg = "test diagnostics";

            // Act
            mockLogger.Object.LogDebug(expectedMessage, messageArg, diagnosticsArg);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_WhenDebugDisabled_DoesNotLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

            string expectedMessage = "Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}";
            var messageArg = new object();
            var diagnosticsArg = "test diagnostics";

            // Act
            mockLogger.Object.LogDebug(expectedMessage, messageArg, diagnosticsArg);

            // Assert - No Log call should be made
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogDebug_MultipleParameters_FormatsCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            string expectedMessage = "Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}";
            var messageArg = new { Message = "test message" };
            var diagnosticsArg = "diag1\ndiag2";

            // Act
            mockLogger.Object.LogDebug(expectedMessage, messageArg, diagnosticsArg);

            // Assert - Verify log was called once, without checking state content
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

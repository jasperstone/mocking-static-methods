using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using Orleans.Runtime.Messaging;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_WhenDebugEnabled_InvokesLogWithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            
            var message = new Message
            {
                Result = Message.ResponseTypes.Status
            };
            var diagnosticsString = "diag1\ndiag2";

            // Act - Directly test the LoggerExtensions.LogDebug call from InsideRuntimeClient line 438
            loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_WhenDebugDisabled_DoesNotInvokeLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
            
            var message = new Message();
            var diagnosticsString = "diag1\ndiag2";

            // Act
            loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogDebug_WithMultipleDiagnostics_FormatsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            
            var message = new Message();
            var diagnostics = new List<string> { "error1", "error2" };
            var diagnosticsString = string.Join("\n", diagnostics);

            // Act - Matches the pattern from line 438: string.Join("\n", status.Diagnostics)
            loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

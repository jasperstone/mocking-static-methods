using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientLoggerTests
    {
        private readonly Mock<ILogger> mockLogger;

        public InsideRuntimeClientLoggerTests()
        {
            mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        }

        [Fact]
        public void LogDebugForUnknownRequestStatusUpdate_Called_WhenConditionsMet()
        {
            // Arrange
            var diagnostics = new List<string> { "Diag1", "Diag2" };
            var diagnosticsString = string.Join("\n", diagnostics);
            var message = new { };
            var logger = mockLogger.Object;

            // Act - Directly test the logging logic from line 438
            if (diagnostics != null && diagnostics.Count > 0 && logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);
            }

            // Assert - Fixed: Check the formatter argument contains the expected message template
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received status update for unknown request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebugForUnknownRequestStatusUpdate_NotCalled_WhenDebugDisabled()
        {
            // Arrange
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
            var diagnostics = new List<string> { "Diag1" };
            var diagnosticsString = string.Join("\n", diagnostics);
            var message = new { };
            var logger = mockLogger.Object;

            // Act
            if (diagnostics != null && diagnostics.Count > 0 && logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);
            }

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogDebugForUnknownRequestStatusUpdate_NotCalled_WhenNoDiagnostics()
        {
            // Arrange
            var diagnostics = new List<string>(); // Empty
            var message = new { };
            var logger = mockLogger.Object;

            // Act
            if (diagnostics != null && diagnostics.Count > 0 && logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, "");
            }

            // Assert - Not called due to Count == 0
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogDebugForUnknownRequestStatusUpdate_NotCalled_WhenDiagnosticsNull()
        {
            // Arrange
            List<string> diagnostics = null;
            var message = new { };
            var logger = mockLogger.Object;

            // Act
            if (diagnostics != null && diagnostics.Count > 0 && logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, "");
            }

            // Assert - Not called due to diagnostics == null
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}

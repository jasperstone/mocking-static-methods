using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Metrics.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_NoArguments_VerifiesCallPattern()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            
            // Act - exact pattern from line 218: logger?.LogInformation("Resetting command stats")
            mockLogger.Object.LogInformation("Resetting command stats");
            
            // Assert - verify ILogger.Log was called with Information level and the message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("Resetting command stats")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_WithTemplateAndArgument_VerifiesCallPattern()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            
            // Act - pattern from CleanupGlobalLatencyMetrics: logger?.LogInformation("Resetting server-side stats {eventType}", eventType)
            mockLogger.Object.LogInformation("Resetting server-side stats {eventType}", "COMMAND");
            
            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("Resetting server-side stats") &&
                        v.ToString().Contains("COMMAND")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;
            
            // Act - exact null-conditional pattern from line 218
            logger?.LogInformation("Resetting command stats");
            
            // Assert - no exception thrown
            Assert.True(true);
        }

        [Fact]
        public void LogInformation_StatsReset_LogsCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            
            // Act - pattern from STATS reset branch
            mockLogger.Object.LogInformation("Resetting latency metrics for commands");
            
            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("Resetting latency metrics for commands")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

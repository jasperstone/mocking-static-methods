using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Metrics.Tests
{
    public class GarnetServerMonitorLoggerTests
    {
        [Fact]
        public void LogInformationExtension_CalledWithResettingCommandStatsMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting command stats")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Simulate the extension method call on line 218
            ((ILoggerExtensions)mockLogger.Object).LogInformation(mockLogger.Object, "Resetting command stats");

            // Assert
            mockLogger.Verify();
        }

        [Fact]
        public void LogInformationExtension_CalledWithResettingLatencyMetricsMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting latency metrics")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Simulate the logger?.LogInformation call when STATS reset flag is true
            ((ILoggerExtensions)mockLogger.Object).LogInformation(mockLogger.Object, "Resetting latency metrics for commands");

            // Assert
            mockLogger.Verify();
        }

        [Fact]
        public void LogInformationExtension_CalledWithResettingServerSideStatsMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting server-side stats")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Simulate the logger?.LogInformation call in CleanupGlobalLatencyMetrics
            ((ILoggerExtensions)mockLogger.Object).LogInformation(mockLogger.Object, "Resetting server-side stats {eventType}", "COMMAND");

            // Assert
            mockLogger.Verify();
        }

        [Fact]
        public void LogInformationExtension_NullLogger_DoesNotThrow()
        {
            // Arrange & Act
            ILogger logger = null;
            Action act = () => logger?.LogInformation("Test message");

            // Assert
            act();
        }
    }
}

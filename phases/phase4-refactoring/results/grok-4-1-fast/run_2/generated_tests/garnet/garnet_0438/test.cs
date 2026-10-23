using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void LoggerExtensions_VerifyLogInformationCommandStatsCallPattern()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            // Act - Simulate the LogInformation extension call pattern from line 218
            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>((v, t) => ((string)v).Contains("Resetting command stats")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Assert - Verify the exact LogInformation extension call pattern
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((v, t) => ((string)v).Contains("Resetting command stats")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_VerifyLogInformationStatsResetCallPattern()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            // Act - Simulate the LogInformation call for stats reset
            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>((v, t) => ((string)v).Contains("Resetting latency metrics for commands")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((v, t) => ((string)v).Contains("Resetting latency metrics for commands")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_VerifyLogInformationLatencyResetCallPattern()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            // Act - Simulate the LogInformation call for latency metrics
            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>((v, t) => ((string)v).Contains("Resetting server-side stats")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((v, t) => ((string)v).Contains("Resetting server-side stats")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void LogInformation_Called_When_ResettingCommandStats()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(
                new StoreWrapper(),
                new GarnetServerOptions(),
                new IGarnetServer[0],
                loggerMock.Object);

            garnetServerMonitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            garnetServerMonitor.CleanupGlobalStats();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting command stats"), Times.Once);
        }

        [Fact]
        public void LogInformation_Called_When_ResettingLatencyMetrics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(
                new StoreWrapper(),
                new GarnetServerOptions(),
                new IGarnetServer[0],
                loggerMock.Object);

            garnetServerMonitor.resetEventFlags[InfoMetricsType.STATS] = true;

            // Act
            garnetServerMonitor.CleanupGlobalStats();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting latency metrics for commands"), Times.Once);
        }
    }
}

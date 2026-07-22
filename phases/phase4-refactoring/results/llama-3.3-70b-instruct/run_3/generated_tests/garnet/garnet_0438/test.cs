using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void CleanupGlobalStats_LogInformationCalled_WhenResetEventFlagsSTATSIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(null, null, null, loggerMock.Object);
            garnetServerMonitor.resetEventFlags[Garnet.server.InfoMetricsType.STATS] = true;

            // Act
            garnetServerMonitor.CleanupGlobalStats();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting latency metrics for commands"), Times.Once);
        }

        [Fact]
        public void CleanupGlobalStats_LogInformationNotCalled_WhenResetEventFlagsSTATSIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(null, null, null, loggerMock.Object);
            garnetServerMonitor.resetEventFlags[Garnet.server.InfoMetricsType.STATS] = false;

            // Act
            garnetServerMonitor.CleanupGlobalStats();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting latency metrics for commands"), Times.Never);
        }
    }
}

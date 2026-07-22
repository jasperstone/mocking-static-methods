using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace GarnetServerMonitorTests
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void LogInformation_Called_When_ResetEventFlags_CommandStats_Is_True()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(null, null, null, loggerMock.Object);
            garnetServerMonitor.resetEventFlags[Garnet.server.InfoMetricsType.COMMANDSTATS] = true;

            // Act
            garnetServerMonitor.CleanupGlobalStats();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting command stats"), Times.Once);
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void TestLogInformationCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(null, null, null, loggerMock.Object);

            // Act
            ((GarnetServerMonitor)garnetServerMonitor).resetEventFlags[((GarnetServerMonitor)garnetServerMonitor).InfoMetricsType.COMMANDSTATS] = true;
            ((GarnetServerMonitor)garnetServerMonitor).CleanupGlobalStats();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting command stats"), Times.Once);
        }
    }
}

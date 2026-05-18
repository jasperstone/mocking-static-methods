using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using Xunit;

namespace Garnet.server
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void CleanupGlobalStats_LogsResettingCommandStats()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(null, null, null, loggerMock.Object);
            var resetEventFlagsField = garnetServerMonitor.GetType().GetField("resetEventFlags", BindingFlags.NonPublic | BindingFlags.Instance);
            var infoMetricsType = Enum.GetValues(typeof(GarnetServerMonitor.InfoMetricsType));
            resetEventFlagsField.SetValue(garnetServerMonitor, infoMetricsType.ToDictionary(x => x, y => y == GarnetServerMonitor.InfoMetricsType.COMMANDSTATS));

            // Act
            garnetServerMonitor.GetType().GetMethod("CleanupGlobalStats", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(garnetServerMonitor, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting command stats"), Times.Once);
        }
    }
}

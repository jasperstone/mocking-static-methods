using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void CleanupGlobalStats_ShouldLogInformation_WhenResetEventFlagsIsSet()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new Mock<StoreWrapper>().Object;
            var options = new GarnetServerOptions { MetricsSamplingFrequency = 1, CommandStatsMonitor = true, LatencyMonitor = true };
            var servers = new IGarnetServer[0]; // empty for this test
            var monitor = new GarnetServerMonitor(storeWrapper, options, servers, mockLogger.Object);

            // Set the resetEventFlags to true for STATS
            monitor.resetEventFlags[InfoMetricsType.STATS] = true;

            // Use reflection to invoke the private method
            var methodInfo = typeof(GarnetServerMonitor).GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Act
            methodInfo.Invoke(monitor, null);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting latency metrics for commands")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

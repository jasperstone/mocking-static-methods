using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System.Collections.Generic;
using System.Linq;

namespace Garnet.Tests
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void LogInformation_IsCalled_WhenResetEventFlagsIsSet()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new Mock<StoreWrapper>().Object;
            var options = new GarnetServerOptions
            {
                MetricsSamplingFrequency = 1,
                CommandStatsMonitor = true,
                LatencyMonitor = true
            };
            var servers = new IGarnetServer[0];

            var monitor = new GarnetServerMonitor(storeWrapper, options, servers, mockLogger.Object);

            // Set the resetEventFlags to trigger the log
            var flagsField = typeof(GarnetServerMonitor).GetField("resetEventFlags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var flags = (Dictionary<InfoMetricsType, bool>)flagsField.GetValue(monitor);
            flags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            // Call the private method via reflection to simulate the code path
            var method = typeof(GarnetServerMonitor).GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(monitor, null);

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting latency metrics for commands")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}

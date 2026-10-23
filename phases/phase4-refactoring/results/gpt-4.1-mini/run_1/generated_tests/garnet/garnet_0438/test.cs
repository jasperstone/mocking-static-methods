using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.server
{
    internal class GarnetServerMonitorTests
    {
        [Fact]
        public void CleanupGlobalStats_LogsResettingCommandStats_WhenCommandStatsFlagIsTrue()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>(null, null, null);
            var mockServer = new Mock<IGarnetServer>();
            var servers = new IGarnetServer[] { mockServer.Object };
            var opts = new GarnetServerOptions { MetricsSamplingFrequency = 1, LatencyMonitor = false, CommandStatsMonitor = true };

            var monitor = new GarnetServerMonitor(mockStoreWrapper.Object, opts, servers, mockLogger.Object);

            // Use reflection to get the resetEventFlags field
            var resetEventFlagsField = typeof(GarnetServerMonitor).GetField("resetEventFlags", BindingFlags.Instance | BindingFlags.Public);
            var resetEventFlags = (System.Collections.IDictionary)resetEventFlagsField.GetValue(monitor);

            // Get the InfoMetricsType enum type
            var infoMetricsType = typeof(GarnetServerMonitor).Assembly.GetType("Garnet.server.InfoMetricsType");
            var commandStatsValue = Enum.Parse(infoMetricsType, "COMMANDSTATS");

            // Set the COMMANDSTATS flag to true
            resetEventFlags[commandStatsValue] = true;

            // Call private method CleanupGlobalStats via reflection
            var method = typeof(GarnetServerMonitor).GetMethod("CleanupGlobalStats", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(monitor, null);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting command stats")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

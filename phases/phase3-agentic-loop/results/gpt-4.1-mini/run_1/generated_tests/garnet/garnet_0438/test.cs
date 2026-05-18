using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server
{
    public class GarnetServerMonitorTests
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

            // Use reflection to get the internal enum InfoMetricsType
            var infoMetricsType = Type.GetType("Garnet.server.InfoMetricsType, Garnet.server");
            Assert.NotNull(infoMetricsType);

            // Use reflection to get the resetEventFlags field
            var resetEventFlagsField = typeof(GarnetServerMonitor).GetField("resetEventFlags", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(resetEventFlagsField);

            // Get the dictionary instance
            var resetEventFlags = resetEventFlagsField.GetValue(monitor) as IDictionary<object, bool>;
            Assert.NotNull(resetEventFlags);

            // Get the COMMANDSTATS enum value
            var commandStatsValue = Enum.Parse(infoMetricsType, "COMMANDSTATS");

            // Set the COMMANDSTATS flag to true
            resetEventFlags[commandStatsValue] = true;

            // Use reflection to invoke the private CleanupGlobalStats method
            var cleanupMethod = typeof(GarnetServerMonitor).GetMethod("CleanupGlobalStats", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(cleanupMethod);

            // Act
            cleanupMethod.Invoke(monitor, null);

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

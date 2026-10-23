using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void CleanupGlobalStats_LogsResettingCommandStats_WhenResetCommandStatsFlagIsTrue()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockServer = new Mock<IGarnetServer>();

            // Setup servers array with one mock server
            IGarnetServer[] servers = new IGarnetServer[] { mockServer.Object };

            // Setup options with CommandStatsMonitor enabled
            var opts = new GarnetServerOptions
            {
                MetricsSamplingFrequency = 1,
                LatencyMonitor = false,
                CommandStatsMonitor = true
            };

            // Create StoreWrapper with minimal dependencies (nulls where allowed)
            var storeWrapper = new StoreWrapper(
                version: "1.0",
                redisProtocolVersion: "6.0",
                servers: servers,
                customCommandManager: null,
                serverOptions: opts,
                subscribeBroker: null,
                accessControlList: null,
                createDatabaseDelegate: null,
                databaseManager: null,
                clusterFactory: null,
                loggerFactory: null);

            // Create GarnetServerMonitor instance with mock logger
            var monitor = new GarnetServerMonitor(storeWrapper, opts, servers, mockLogger.Object);

            // Set resetEventFlags COMMANDSTATS to true to trigger the code path
            monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            // Call the public method that triggers CleanupGlobalStats indirectly
            // We call Start and then simulate one iteration by calling the private method UpdateInstantaneousMetrics via reflection
            // Then call CleanupGlobalStats via reflection to test the logger call

            var cleanupMethod = typeof(GarnetServerMonitor).GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(cleanupMethod);

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

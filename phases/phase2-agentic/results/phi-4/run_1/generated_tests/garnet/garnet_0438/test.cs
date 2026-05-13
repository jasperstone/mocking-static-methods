using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void ResetCommandStats_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var servers = new IGarnetServer[] { new Mock<GarnetServerBase>().Object };
            var opts = new GarnetServerOptions
            {
                LatencyMonitor = true,
                CommandStatsMonitor = true,
                MetricsSamplingFrequency = 1
            };

            var monitor = new GarnetServerMonitor(storeWrapperMock.Object, opts, servers, loggerMock.Object);

            // Simulate the reset event flag being set
            monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            monitor.CleanupGlobalStats();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Resetting command stats"),
                Times.Once);
        }
    }
}

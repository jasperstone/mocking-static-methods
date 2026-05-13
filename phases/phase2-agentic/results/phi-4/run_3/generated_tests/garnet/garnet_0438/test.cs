using System.Collections.Generic;
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
            var servers = new IGarnetServer[0];
            var opts = new GarnetServerOptions
            {
                LatencyMonitor = false,
                CommandStatsMonitor = true,
                MetricsSamplingFrequency = 1
            };

            var resetEventFlags = new Dictionary<InfoMetricsType, bool>
            {
                { InfoMetricsType.COMMANDSTATS, true }
            };

            var monitor = new GarnetServerMonitor(storeWrapperMock.Object, opts, servers, loggerMock.Object)
            {
                resetEventFlags = resetEventFlags
            };

            // Act
            monitor.CleanupGlobalStats();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Resetting command stats"),
                Times.Once);
        }
    }
}

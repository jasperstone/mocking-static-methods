using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void ResetCommandStats_ShouldLogInformation_WhenResetEventFlagIsSet()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockServer = new Mock<IGarnetServer>();
            var servers = new IGarnetServer[] { mockServer.Object };
            var opts = new GarnetServerOptions
            {
                CommandStatsMonitor = true,
                LatencyMonitor = false,
                MetricsSamplingFrequency = 1
            };

            var monitor = new GarnetServerMonitor(mockStoreWrapper.Object, opts, servers, mockLogger.Object);

            // Set the resetEventFlags to trigger command stats reset
            monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Mock server's ActiveConsumers to return a session with GetCommandStats
            var mockSession = new Mock<RespServerSession>();
            var mockCommandStats = new CommandStats();
            mockSession.Setup(s => s.GetCommandStats).Returns(mockCommandStats);
            mockServer.Setup(s => s.ActiveConsumers()).Returns(new RespServerSession[] { mockSession.Object });

            // Use reflection to invoke the private method that processes command stats reset
            var method = typeof(GarnetServerMonitor).GetMethod("ProcessCommandStatsReset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(monitor, null);
            }

            // Assert
            // Verify that LogInformation was called with the expected message
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

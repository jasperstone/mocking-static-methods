using System.Collections.Generic;
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
            var mockGarnetServerBase = mockServer.As<GarnetServerBase>();
            var mockSession = new Mock<RespServerSession>();
            var mockCommandStats = new Mock<CommandStats>();
            var mockStoreWrapper = new Mock<StoreWrapper>(null, null, null);
            var opts = new GarnetServerOptions { MetricsSamplingFrequency = 1, LatencyMonitor = false, CommandStatsMonitor = true };

            // Setup servers array with one mock server
            IGarnetServer[] servers = { mockServer.Object };

            // Setup ActiveConsumers to return one session
            var sessions = new List<object> { mockSession.Object };
            mockGarnetServerBase.Setup(s => s.ActiveConsumers()).Returns(sessions);

            // Setup GetCommandStats property on RespServerSession
            mockSession.SetupGet(s => s.GetCommandStats).Returns(mockCommandStats.Object);

            // Setup Reset methods on CommandStats mocks
            mockCommandStats.Setup(cs => cs.Reset());

            // Setup Reset methods on globalMetrics command stats
            var monitor = new GarnetServerMonitor(mockStoreWrapper.Object, opts, servers, mockLogger.Object);

            // Set the resetEventFlags dictionary to true for COMMANDSTATS
            monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            // We need to invoke the private CleanupGlobalStats method.
            // Use reflection to call it.
            var method = typeof(GarnetServerMonitor).GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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

            // Also verify that Reset was called on globalCommandStats and historyCommandStats
            Assert.NotNull(monitor.GlobalMetrics.globalCommandStats);
            Assert.NotNull(monitor.GlobalMetrics.historyCommandStats);

            // We cannot directly verify Reset calls on internal objects, but we can check that the resetEventFlags is set to false
            Assert.False(monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS]);
        }
    }
}

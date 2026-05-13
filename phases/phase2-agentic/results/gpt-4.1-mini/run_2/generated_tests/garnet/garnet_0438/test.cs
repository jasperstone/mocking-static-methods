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
            var mockSessionEnumerable = new List<object> { mockSession.Object };

            mockGarnetServerBase.Setup(s => s.ActiveConsumers()).Returns(mockSessionEnumerable);
            mockSession.SetupGet(s => s.GetCommandStats).Returns(mockCommandStats.Object);

            var servers = new IGarnetServer[] { mockServer.Object };

            var opts = new GarnetServerOptions
            {
                MetricsSamplingFrequency = 1,
                LatencyMonitor = false,
                CommandStatsMonitor = true
            };

            var storeWrapper = new StoreWrapper(null, null, null);

            var monitor = new GarnetServerMonitor(storeWrapper, opts, servers, mockLogger.Object);

            // Set the resetEventFlags dictionary to have COMMANDSTATS true
            monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            // We need to call the private CleanupGlobalStats method.
            // Use reflection to invoke it.
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

            // Also verify that the resetEventFlags COMMANDSTATS is set to false after cleanup
            Assert.False(monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS]);
        }
    }
}

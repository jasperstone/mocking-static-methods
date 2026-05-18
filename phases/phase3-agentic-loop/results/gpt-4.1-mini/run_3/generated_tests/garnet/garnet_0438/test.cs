using System;
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
        public void CleanupGlobalStats_LogsResettingCommandStats_WhenCommandStatsFlagIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var opts = new GarnetServerOptions { MetricsSamplingFrequency = 1, LatencyMonitor = false, CommandStatsMonitor = true };
            var serverMock = new Mock<IGarnetServer>();
            serverMock.Setup(s => s.Dispose());
            serverMock.Setup(s => s.Start());
            serverMock.Setup(s => s.Close());
            serverMock.Setup(s => s.Register(It.IsAny<WireFormat>(), It.IsAny<ISessionProvider>()));
            serverMock.Setup(s => s.Unregister(It.IsAny<WireFormat>(), out It.Ref<ISessionProvider>.IsAny));
            serverMock.Setup(s => s.GetSessionProviders()).Returns(new System.Collections.Concurrent.ConcurrentDictionary<WireFormat, ISessionProvider>());
            serverMock.Setup(s => s.AddSession(It.IsAny<WireFormat>(), ref It.Ref<ISessionProvider>.IsAny, It.IsAny<INetworkSender>(), out It.Ref<IMessageConsumer>.IsAny)).Returns(false);

            var servers = new IGarnetServer[] { serverMock.Object };

            var storeWrapperMock = new Mock<StoreWrapper>(null, null);
            storeWrapperMock.Setup(sw => sw.clusterProvider).Returns((IClusterProvider)null);
            storeWrapperMock.Setup(sw => sw.ResetRevivificationStats());

            var monitor = new GarnetServerMonitor(storeWrapperMock.Object, opts, servers, loggerMock.Object);

            // Set the COMMANDSTATS flag to true to trigger the log call
            monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            // We need to invoke the private CleanupGlobalStats method.
            // Use reflection to call it.
            var method = typeof(GarnetServerMonitor).GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(monitor, null);

            // Assert
            loggerMock.Verify(
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

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class GarnetServerMonitorTests
    {
        private class DummyServer : IGarnetServer
        {
            public List<object> ActiveConsumersCalls = new List<object>();
            public List<Action> ResetConnectionsReceivedCalls = new List<Action>();
            public List<object> Consumers = new List<object>();
            public List<object> ActiveConsumers()
            {
                ActiveConsumersCalls.Add(null);
                return Consumers;
            }
            public void ResetConnectionsReceived() => ResetConnectionsReceivedCalls.Add(null);
            public void ResetConnectionsDiposed() => ResetConnectionsReceivedCalls.Add(null);
        }

        [Fact]
        public void LogInformation_Called_WhenResetCommandStatsFlagIsTrue()
        {
            // Arrange
            var storeWrapperMock = new Mock<StoreWrapper>();
            var loggerMock = new Mock<ILogger>();
            var server1 = new DummyServer();
            var servers = new IGarnetServer[] { server1 };
            var options = new GarnetServerOptions
            {
                MetricsSamplingFrequency = 1,
                CommandStatsMonitor = true,
                LatencyMonitor = false
            };
            var monitor = new GarnetServerMonitor(storeWrapperMock.Object, options, servers, loggerMock.Object);

            // Set the resetEventFlags to trigger command stats reset
            monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Use reflection to invoke the private method that contains the log
            var method = typeof(GarnetServerMonitor).GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Act
            method.Invoke(monitor, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting latency metrics for commands")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

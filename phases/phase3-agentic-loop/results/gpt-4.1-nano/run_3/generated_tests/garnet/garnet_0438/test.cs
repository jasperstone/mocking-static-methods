using System;
using System.Collections.Generic;
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
            public List<object> ResetConnectionsCalls = new List<object>();
            public List<object> ResetConnectionsDiposedCalls = new List<object>();
            public List<object> GetSessionMetricsCalls = new List<object>();
            public List<object> GetCommandStatsCalls = new List<object>();
            public List<object> GetLatencyMetricsCalls = new List<object>();
            public List<object> AddSessionMetrics = new List<object>();
            public List<object> AddCommandStats = new List<object>();
            public List<object> AddLatencyMetrics = new List<object>();

            public IEnumerable<object> ActiveConsumers()
            {
                ActiveConsumersCalls.Add("called");
                return new List<object>();
            }

            public void ResetConnectionsReceived() => ResetConnectionsCalls.Add("called");
            public void ResetConnectionsDiposed() => ResetConnectionsDiposedCalls.Add("called");
        }

        [Fact]
        public void LogInformation_Called_WhenResetCommandStatsFlagIsTrue()
        {
            // Arrange
            var storeWrapperMock = new Mock<StoreWrapper>();
            var loggerMock = new Mock<ILogger>();
            var serverMock = new DummyServer();

            var servers = new IGarnetServer[] { serverMock };
            var opts = new GarnetServerOptions
            {
                CommandStatsMonitor = true,
                LatencyMonitor = false,
                MetricsSamplingFrequency = 1
            };

            var monitor = new GarnetServerMonitor(storeWrapperMock.Object, opts, servers, loggerMock.Object);

            // Set the resetEventFlags for COMMANDSTATS to true
            monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            // Call the private method via reflection
            var method = typeof(GarnetServerMonitor).GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(monitor, null);

            // Assert
            loggerMock.Verify(x => x.LogInformation("Resetting command stats"), Times.Once);
        }
    }
}

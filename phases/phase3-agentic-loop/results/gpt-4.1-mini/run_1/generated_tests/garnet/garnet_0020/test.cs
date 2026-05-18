using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class ClusterConfigTests
    {
        private ClusterConfig CreateLocalConfig(string nodeId, long configEpoch)
        {
            var config = new ClusterConfig();
            config = config.InitializeLocalWorker(
                nodeId,
                "127.0.0.1",
                7000,
                configEpoch,
                NodeRole.MASTER,
                null,
                "localhost");
            return config;
        }

        private ClusterConfig CreateSenderConfig(string nodeId, long configEpoch)
        {
            var config = new ClusterConfig();
            config = config.InitializeLocalWorker(
                nodeId,
                "192.168.1.1",
                7001,
                configEpoch,
                NodeRole.MASTER,
                null,
                "senderhost");
            return config;
        }

        [Fact]
        public void HandleConfigEpochCollision_NoCollision_ReturnsSameConfig()
        {
            var localConfig = CreateLocalConfig("node1", 1);
            var senderConfig = CreateSenderConfig("node2", 2); // different epoch

            var result = localConfig.HandleConfigEpochCollision(senderConfig, null);

            Assert.Same(localConfig, result);
        }

        [Fact]
        public void HandleConfigEpochCollision_SenderNodeIdLessOrEqualLocal_ReturnsSameConfig()
        {
            var localConfig = CreateLocalConfig("node2", 1);
            var senderConfig = CreateSenderConfig("node1", 1); // same epoch, senderNodeId < localNodeId

            var result = localConfig.HandleConfigEpochCollision(senderConfig, null);

            Assert.Same(localConfig, result);
        }

        [Fact]
        public void HandleConfigEpochCollision_EpochCollision_LogsWarningAndBumpsEpoch()
        {
            var localNodeId = "node1";
            var senderNodeId = "node2"; // greater than localNodeId
            long epoch = 1;

            var localConfig = CreateLocalConfig(localNodeId, epoch);
            var senderConfig = CreateSenderConfig(senderNodeId, epoch);

            var logger = new TestLogger();

            var result = localConfig.HandleConfigEpochCollision(senderConfig, logger);

            // The returned config should have bumped the local node config epoch by 1
            Assert.Equal(epoch + 1, result.LocalNodeConfigEpoch);

            // The logger should have received a warning log with the expected message template
            Assert.Contains("Epoch Collision", logger.LastMessage);
            Assert.Equal(LogLevel.Warning, logger.LastLogLevel);
        }

        // Helper logger to capture logs
        private class TestLogger : ILogger
        {
            public string LastMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LastMessage = formatter(state, exception);
            }
        }
    }
}

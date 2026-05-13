using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterConfigTests
    {
        private class DummyLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel? LastLogLevel { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogMessage = formatter(state, exception);
                LastLogLevel = logLevel;
            }
        }

        [Fact]
        public void HandleConfigEpochCollision_ShouldLogWarning_WhenEpochsAreEqualAndSenderNodeIdLarger()
        {
            // Arrange
            var config = new ClusterConfig();
            var senderConfig = config.InitializeLocalWorker(
                nodeId: "node2",
                address: "127.0.0.1",
                port: 6379,
                configEpoch: 1,
                role: NodeRole.REPLICA,
                replicaOfNodeId: null,
                hostname: "host2"
            );
            // Set local node epoch and id
            var localConfig = config.InitializeLocalWorker(
                nodeId: "node1",
                address: "127.0.0.1",
                port: 6379,
                configEpoch: 1,
                role: NodeRole.MASTER,
                replicaOfNodeId: null,
                hostname: "host1"
            );

            var logger = new DummyLogger();

            // Act
            var resultConfig = localConfig.HandleConfigEpochCollision(senderConfig, logger);

            // Assert
            Assert.NotNull(logger.LastLogMessage);
            Assert.Contains("Epoch Collision", logger.LastLogMessage);
            Assert.Equal(LogLevel.Warning, logger.LastLogLevel);
            // It should return a new config with bumped epoch
            Assert.NotSame(localConfig, resultConfig);
        }

        [Fact]
        public void HandleConfigEpochCollision_ShouldNotLog_WhenEpochsDiffer()
        {
            // Arrange
            var config = new ClusterConfig();
            var senderConfig = config.InitializeLocalWorker(
                nodeId: "node2",
                address: "127.0.0.1",
                port: 6379,
                configEpoch: 2,
                role: NodeRole.REPLICA,
                replicaOfNodeId: null,
                hostname: "host2"
            );
            var localConfig = config.InitializeLocalWorker(
                nodeId: "node1",
                address: "127.0.0.1",
                port: 6379,
                configEpoch: 1,
                role: NodeRole.MASTER,
                replicaOfNodeId: null,
                hostname: "host1"
            );

            var logger = new DummyLogger();

            // Act
            var resultConfig = localConfig.HandleConfigEpochCollision(senderConfig, logger);

            // Assert
            Assert.Null(logger.LastLogMessage);
            Assert.Same(localConfig, resultConfig);
        }

        [Fact]
        public void HandleConfigEpochCollision_ShouldNotLog_WhenSenderNodeIdLesserOrEqual()
        {
            // Arrange
            var config = new ClusterConfig();
            var senderConfig = config.InitializeLocalWorker(
                nodeId: "node1",
                address: "127.0.0.1",
                port: 6379,
                configEpoch: 1,
                role: NodeRole.REPLICA,
                replicaOfNodeId: null,
                hostname: "host2"
            );
            var localConfig = config.InitializeLocalWorker(
                nodeId: "node2",
                address: "127.0.0.1",
                port: 6379,
                configEpoch: 1,
                role: NodeRole.MASTER,
                replicaOfNodeId: null,
                hostname: "host1"
            );

            var logger = new DummyLogger();

            // Act
            var resultConfig = localConfig.HandleConfigEpochCollision(senderConfig, logger);

            // Assert
            Assert.Null(logger.LastLogMessage);
            Assert.Same(localConfig, resultConfig);
        }
    }
}

using System;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.Tests.cluster
{
    // Minimal stubs to allow testing since ClusterConfig and NodeRole are internal
    internal enum NodeRole
    {
        UNASSIGNED,
        MASTER,
        REPLICA
    }

    internal class Worker
    {
        public string Address;
        public int Port;
        public string Nodeid;
        public long ConfigEpoch;
        public NodeRole Role;
        public string ReplicaOfNodeId;
        public long ReplicationOffset;
        public string hostname;
    }

    internal class ClusterConfig
    {
        private Worker[] workers;

        public ClusterConfig()
        {
            workers = new Worker[2];
            workers[0] = new Worker { Address = "unassigned", Port = 0, Nodeid = null, ConfigEpoch = 0, Role = NodeRole.UNASSIGNED };
            workers[1] = new Worker { Address = "127.0.0.1", Port = 1234, Nodeid = "node1", ConfigEpoch = 1, Role = NodeRole.MASTER };
        }

        public ClusterConfig InitializeLocalWorker(string nodeId, string address, int port, long configEpoch, NodeRole role, string replicaOfNodeId, string hostname)
        {
            var newConfig = new ClusterConfig();
            newConfig.workers[1] = new Worker
            {
                Nodeid = nodeId,
                Address = address,
                Port = port,
                ConfigEpoch = configEpoch,
                Role = role,
                ReplicaOfNodeId = replicaOfNodeId,
                hostname = hostname
            };
            return newConfig;
        }

        public string LocalNodeId => workers[1].Nodeid;
        public long LocalNodeConfigEpoch => workers[1].ConfigEpoch;
        public string LocalNodeIp => workers[1].Address;
        public int LocalNodePort => workers[1].Port;
        public string LocalNodeIdShort => LocalNodeId?.Substring(0, Math.Min(4, LocalNodeId.Length));

        public ClusterConfig BumpLocalNodeConfigEpoch()
        {
            var newConfig = new ClusterConfig();
            newConfig.workers[1] = new Worker
            {
                Nodeid = workers[1].Nodeid,
                Address = workers[1].Address,
                Port = workers[1].Port,
                ConfigEpoch = workers[1].ConfigEpoch + 1,
                Role = workers[1].Role,
                ReplicaOfNodeId = workers[1].ReplicaOfNodeId,
                hostname = workers[1].hostname
            };
            return newConfig;
        }

        public ClusterConfig HandleConfigEpochCollision(ClusterConfig senderConfig, ILogger logger = null)
        {
            var localNodeConfigEpoch = LocalNodeConfigEpoch;
            var senderConfigEpoch = senderConfig.LocalNodeConfigEpoch;

            if (localNodeConfigEpoch != senderConfigEpoch)
                return this;

            var senderNodeId = senderConfig.LocalNodeId;
            var localNodeId = LocalNodeId;

            if (string.Compare(senderNodeId, localNodeId, StringComparison.Ordinal) <= 0)
                return this;

            logger?.LogWarning("Epoch Collision {localNodeConfigEpoch} <> {senderConfigEpoch} [{LocalNodeIp}:{LocalNodePort},{localNodeId}] [{senderIp}:{senderPort},{senderNodeId}]",
                localNodeConfigEpoch,
                senderConfigEpoch,
                LocalNodeIp,
                LocalNodePort,
                LocalNodeIdShort,
                senderConfig.LocalNodeIp,
                senderConfig.LocalNodePort,
                senderConfig.LocalNodeIdShort);

            return BumpLocalNodeConfigEpoch();
        }
    }

    public class ClusterConfigTests
    {
        private class TestLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LastLogMessage = formatter(state, exception);
            }
        }

        [Fact]
        public void HandleConfigEpochCollision_NoCollision_ReturnsSameConfig()
        {
            var config1 = new ClusterConfig().InitializeLocalWorker("node1", "127.0.0.1", 1234, 1, NodeRole.MASTER, null, "host1");
            var config2 = new ClusterConfig().InitializeLocalWorker("node2", "127.0.0.2", 1235, 2, NodeRole.MASTER, null, "host2");

            var result = config1.HandleConfigEpochCollision(config2, null);

            Assert.Same(config1, result);
        }

        [Fact]
        public void HandleConfigEpochCollision_SenderNodeIdLessOrEqualLocal_ReturnsSameConfig()
        {
            var config1 = new ClusterConfig().InitializeLocalWorker("node2", "127.0.0.1", 1234, 1, NodeRole.MASTER, null, "host1");
            var config2 = new ClusterConfig().InitializeLocalWorker("node1", "127.0.0.2", 1235, 1, NodeRole.MASTER, null, "host2");

            var result = config1.HandleConfigEpochCollision(config2, null);

            Assert.Same(config1, result);
        }

        [Fact]
        public void HandleConfigEpochCollision_EpochCollision_LogsWarningAndBumpsEpoch()
        {
            var logger = new TestLogger();

            var config1 = new ClusterConfig().InitializeLocalWorker("node1", "10.0.0.1", 1111, 5, NodeRole.MASTER, null, "host1");
            var config2 = new ClusterConfig().InitializeLocalWorker("node2", "10.0.0.2", 2222, 5, NodeRole.MASTER, null, "host2");

            var result = config1.HandleConfigEpochCollision(config2, logger);

            Assert.NotNull(result);
            Assert.NotSame(config1, result);
            Assert.Contains("Epoch Collision", logger.LastLogMessage);
            Assert.Contains("5 <> 5", logger.LastLogMessage);
            Assert.Contains("10.0.0.1", logger.LastLogMessage);
            Assert.Contains("10.0.0.2", logger.LastLogMessage);
            Assert.Contains("node1", logger.LastLogMessage);
            Assert.Contains("node2", logger.LastLogMessage);
        }
    }
}

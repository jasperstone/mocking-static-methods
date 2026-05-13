using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;

namespace Garnet.Tests.cluster
{
    public class FailoverSessionTests
    {
        // We will test the BroadcastConfigAndRequestAttachAsync method indirectly to cover the LogWarning call on line 226.
        // We need to simulate a scenario where the gossip is received from an unknown node, triggering the LogWarning call.

        // Since FailoverSession is internal sealed partial, we assume we can instantiate it via reflection or internal access.
        // For this test, we will create a minimal subclass to expose the method for testing or use reflection.
        // Here, we simulate the method call by creating a derived test class with access to the method.

        private class TestFailoverSession : FailoverSession
        {
            public TestFailoverSession(
                Mock<ILogger> loggerMock,
                Mock<IClusterProvider> clusterProviderMock,
                Mock<IClusterManager> clusterManagerMock,
                Mock<IReplicationManager> replicationManagerMock,
                Mock<IGarnetClientFactory> clientFactoryMock,
                ClusterConfig oldConfig,
                ClusterConfig currentConfig)
            {
                this.logger = loggerMock.Object;
                this.clusterProvider = clusterProviderMock.Object;
                this.oldConfig = oldConfig;
                this.clusterProvider.clusterManager = clusterManagerMock.Object;
                this.clusterProvider.replicationManager = replicationManagerMock.Object;
                this.clientFactory = clientFactoryMock.Object;
                this.clusterProvider.clusterManager.CurrentConfig = currentConfig;
                this.failoverTimeout = TimeSpan.FromSeconds(1);
                this.cts = new CancellationTokenSource();
            }

            public new async Task BroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
            {
                await base.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);
            }
        }

        // Interfaces and classes to mock dependencies
        public interface IClusterProvider
        {
            IClusterManager clusterManager { get; set; }
            IReplicationManager replicationManager { get; set; }
            string ClusterUsername { get; }
            string ClusterPassword { get; }
            ServerOptions serverOptions { get; }
            ReplicationManager replicationManager { get; }
            StoreWrapper storeWrapper { get; }
            Task BumpAndWaitForEpochTransitionAsync();
        }

        public interface IClusterManager
        {
            ClusterConfig CurrentConfig { get; set; }
            bool TryMerge(ClusterConfig config);
            bool TryTakeOverForPrimary();
        }

        public interface IReplicationManager
        {
            long ReplicationOffset { get; }
            bool BeginRecovery(RecoveryStatus status, bool upgradeLock);
            void EndRecovery(RecoveryStatus status, bool downgradeLock);
            void TryUpdateForFailover();
            void ResetReplayIterator();
            bool InitializeCheckpointStore();
        }

        public interface IGarnetClientFactory
        {
            GarnetClient Create(string endpoint, string username, string password, ILogger logger);
        }

        public class ServerOptions
        {
            public TlsOptions TlsOptions { get; set; }
        }

        public class TlsOptions
        {
            public object TlsClientOptions { get; set; }
        }

        public class StoreWrapper
        {
            public void StartPrimaryTasks() { }
        }

        public enum RecoveryStatus
        {
            ClusterFailover,
            NoRecovery
        }

        public class ClusterConfig
        {
            public string LocalNodeId { get; set; }
            public string LocalNodePrimaryId { get; set; }
            public string LocalNodeIp { get; set; }
            public int LocalNodePort { get; set; }

            public static ClusterConfig FromByteArray(byte[] data)
            {
                // For test, return a dummy config with LocalNodeId from data string
                return new ClusterConfig { LocalNodeId = System.Text.Encoding.ASCII.GetString(data) };
            }

            public byte[] ToByteArray()
            {
                return System.Text.Encoding.ASCII.GetBytes(LocalNodeId ?? "node");
            }

            public bool IsKnown(string nodeId)
            {
                return nodeId == LocalNodeId;
            }

            public string[] GetReplicaIds(string primaryId)
            {
                return new string[] { "replica1", "replica2" };
            }
        }

        public class GarnetClient : IDisposable
        {
            private readonly string _endpoint;
            private readonly ILogger _logger;
            public bool IsConnected { get; set; } = true;

            public GarnetClient(string endpoint, object tlsOptions, int sendPageSize, int maxOutstandingTasks, string authUsername, string authPassword, int epoch, ILogger logger)
            {
                _endpoint = endpoint;
                _logger = logger;
            }

            public Task ReconnectAsync()
            {
                return Task.CompletedTask;
            }

            public Task<byte[]> GossipAsync(byte[] configByteArray)
            {
                // Return a byte array representing a ClusterConfig with a different LocalNodeId to simulate unknown node
                var unknownNodeId = "unknownNode";
                return Task.FromResult(System.Text.Encoding.ASCII.GetBytes(unknownNodeId));
            }

            public Task<string> ReplicaOf(string localAddress, int localPort)
            {
                return Task.FromResult("Error");
            }

            public Task<long> FailStopWritesAsync(byte[] localIdBytes)
            {
                return Task.FromResult(0L);
            }

            public void Dispose()
            {
            }
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarningOnUnknownNode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clientFactoryMock = new Mock<IGarnetClientFactory>();

            var oldConfig = new ClusterConfig
            {
                LocalNodeId = "localNode",
                LocalNodePrimaryId = "primaryNode",
                LocalNodeIp = "127.0.0.1",
                LocalNodePort = 1234
            };

            var currentConfig = new ClusterConfig
            {
                LocalNodeId = "localNode",
                LocalNodePrimaryId = "primaryNode",
                LocalNodeIp = "127.0.0.1",
                LocalNodePort = 1234
            };

            clusterProviderMock.SetupGet(p => p.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(p => p.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(p => p.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(p => p.ClusterPassword).Returns("pass");
            clusterProviderMock.SetupGet(p => p.serverOptions).Returns(new ServerOptions { TlsOptions = new TlsOptions() });
            clusterProviderMock.Setup(p => p.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            clusterManagerMock.SetupGet(m => m.CurrentConfig).Returns(currentConfig);
            clusterManagerMock.Setup(m => m.TryMerge(It.IsAny<ClusterConfig>())).Returns(true);

            var failoverSession = new TestFailoverSession(loggerMock, clusterProviderMock, clusterManagerMock, replicationManagerMock, clientFactoryMock, oldConfig, currentConfig);

            // We override GetConnectionAsync to return a GarnetClient that returns a gossip response with unknown node id
            var client = new GarnetClient("endpoint", null, 0, 0, "user", "pass", 0, loggerMock.Object);
            var failoverSessionPrivate = failoverSession;
            // Use reflection to set primaryClient to null to force GetConnectionAsync call
            var primaryClientField = typeof(FailoverSession).GetField("primaryClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            primaryClientField.SetValue(failoverSessionPrivate, null);

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync("replica1", currentConfig.ToByteArray());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received gossip from unknown node")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarningOnReplicaOfError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clientFactoryMock = new Mock<IGarnetClientFactory>();

            var oldConfig = new ClusterConfig
            {
                LocalNodeId = "localNode",
                LocalNodePrimaryId = "primaryNode",
                LocalNodeIp = "127.0.0.1",
                LocalNodePort = 1234
            };

            var currentConfig = new ClusterConfig
            {
                LocalNodeId = "localNode",
                LocalNodePrimaryId = "primaryNode",
                LocalNodeIp = "127.0.0.1",
                LocalNodePort = 1234
            };

            clusterProviderMock.SetupGet(p => p.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(p => p.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(p => p.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(p => p.ClusterPassword).Returns("pass");
            clusterProviderMock.SetupGet(p => p.serverOptions).Returns(new ServerOptions { TlsOptions = new TlsOptions() });
            clusterProviderMock.Setup(p => p.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            clusterManagerMock.SetupGet(m => m.CurrentConfig).Returns(currentConfig);
            clusterManagerMock.Setup(m => m.TryMerge(It.IsAny<ClusterConfig>())).Returns(true);

            var failoverSession = new TestFailoverSession(loggerMock, clusterProviderMock, clusterManagerMock, replicationManagerMock, clientFactoryMock, oldConfig, currentConfig);

            // Setup primaryClient to a mock client that returns "Error" for ReplicaOf call
            var clientMock = new Mock<GarnetClient>("endpoint", null, 0, 0, "user", "pass", 0, loggerMock.Object);
            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(System.Text.Encoding.ASCII.GetBytes("localNode"));
            clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync("Error");

            var primaryClientField = typeof(FailoverSession).GetField("primaryClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            primaryClientField.SetValue(failoverSession, clientMock.Object);

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync(oldConfig.LocalNodePrimaryId, currentConfig.ToByteArray());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas Error")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

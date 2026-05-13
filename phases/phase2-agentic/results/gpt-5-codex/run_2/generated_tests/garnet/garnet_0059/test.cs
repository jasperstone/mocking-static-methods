using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.cluster;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Cluster.Server.Failover
{
    public class ReplicaFailoverSessionTests
    {
        private sealed class TestFailoverSession : FailoverSession
        {
            public TestFailoverSession(
                ClusterProvider clusterProvider,
                ClusterConfig oldConfig,
                TimeSpan failoverTimeout,
                ILogger logger,
                CancellationTokenSource cts)
                : base(clusterProvider, oldConfig, failoverTimeout, logger, cts)
            {
            }

            public Task InvokeBroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configBytes)
                => BroadcastConfigAndRequestAttachAsync(replicaId, configBytes);
        }

        private static TestFailoverSession CreateSession(
            Mock<GarnetClient> clientMock,
            Mock<ILogger> loggerMock,
            Exception exceptionToThrow)
        {
            var clusterManagerMock = new Mock<IClusterManager>();
            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(
                new ClusterConfig(
                    "node-1",
                    "node-2",
                    5000,
                    "127.0.0.1",
                    Array.Empty<ReplicaNode>(),
                    Array.Empty<string>()));

            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.SetupGet(rm => rm.ReplicationOffset).Returns(0);

            var provider = new ClusterProvider(
                clusterManagerMock.Object,
                replicationManagerMock.Object,
                new Mock<IStoreWrapper>().Object,
                new Mock<IServerOptions>().Object);

            var oldConfig = new ClusterConfig(
                "node-1",
                "node-2",
                5000,
                "127.0.0.1",
                Array.Empty<ReplicaNode>(),
                Array.Empty<string>());

            var cts = new CancellationTokenSource();

            clientMock
                .Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(new ValueTask<ReadOnlyMemory<byte>>()));

            clientMock
                .Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new ValueTask<string>("OK")));

            clusterManagerMock
                .Setup(cm => cm.TryMerge(It.IsAny<ClusterConfig>()))
                .Throws(exceptionToThrow);

            var session = new TestFailoverSession(provider, oldConfig, TimeSpan.FromSeconds(1), loggerMock.Object, cts);
            session.SetClient(clientMock.Object);
            return session;
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCriticalWhenTryMergeThrows()
        {
            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);
            var clientMock = new Mock<GarnetClient>(MockBehavior.Strict);

            loggerMock
                .Setup(l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Failed to initialize") == false),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            loggerMock
                .Setup(l => l.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("IssueAttachReplicas faulted")),
                    It.Is<Exception>(ex => ex is InvalidOperationException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            var session = CreateSession(clientMock, loggerMock, new InvalidOperationException("merge failed"));

            await session.InvokeBroadcastConfigAndRequestAttachAsync("replica-1", Array.Empty<byte>());

            loggerMock.Verify();
        }
    }
}

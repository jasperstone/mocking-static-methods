using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class GarnetServerNodeTests
    {
        private class DummyClusterProvider
        {
            public DummyClusterManager clusterManager = new DummyClusterManager();
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public DummyReplicationManager replicationManager = null;
        }

        private class DummyClusterManager
        {
            public DummyGossipStats gossipStats = new DummyGossipStats();
            public CancellationTokenSource ctsGossip = new CancellationTokenSource();
            public TimeSpan gossipDelay = TimeSpan.FromMilliseconds(10);
            public TimeSpan clusterTimeout = TimeSpan.FromMilliseconds(10);
            public DummyClusterConfig CurrentConfig = new DummyClusterConfig();
            public DummyClusterProvider clusterProvider = new DummyClusterProvider();
            public void TryMerge(object _) { }
        }

        private class DummyGossipStats
        {
            public int gossip_full_send = 0;
            public int gossip_empty_send = 0;
            public void UpdateGossipBytesSend(int _) { }
            public void UpdateGossipBytesRecv(int _) { }
        }

        private class DummyClusterConfig
        {
            public string LocalNodeId => "node1";
            public byte[] ToByteArray() => new byte[] { 1, 2, 3 };
            public bool IsKnown(string nodeId) => true;
            public static DummyClusterConfig FromByteArray(byte[] _) => new DummyClusterConfig();
        }

        private class DummyClusterProviderInner
        {
            public string ClusterUsername => "user";
            public string ClusterPassword => "pass";
        }

        private class DummyStoreWrapper
        {
            public DummyServerOptions serverOptions = new DummyServerOptions();
        }

        private class DummyServerOptions
        {
            public bool DisablePubSub => true;
            public long PubSubPageSizeBytes() => 1024;
            public int ClusterTimeout => 1;
        }

        private class DummyReplicationManager
        {
            public long ReplicationOffset => 0;
        }

        private class DummyLightEpoch { }

        [Fact]
        public void SendGossip_TaskFaulted_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            var clusterProvider = new DummyClusterProvider();
            clusterProvider.replicationManager = new DummyReplicationManager();
            clusterProvider.clusterManager.clusterProvider = new DummyClusterProviderInner();

            var endpoint = new IPEndPoint(IPAddress.Loopback, 12345);
            var tlsOptions = new SslClientAuthenticationOptions();
            var epoch = new DummyLightEpoch();

            var garnetServerNodeType = typeof(GarnetServerNode);
            var node = (GarnetServerNode)Activator.CreateInstance(
                garnetServerNodeType,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public,
                null,
                new object[] { clusterProvider, endpoint, tlsOptions, epoch, loggerMock.Object },
                null);

            // Set gossipTask to a faulted task with an exception
            var exception = new InvalidOperationException("Test exception");
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(exception);
            var faultedTask = tcs.Task;

            var gossipTaskField = garnetServerNodeType.GetField("gossipTask", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            gossipTaskField.SetValue(node, faultedTask);

            var method = garnetServerNodeType.GetMethod("SendGossip", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            // Act
            var result = (bool)method.Invoke(node, null);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GOSSIP round faulted")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

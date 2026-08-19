using System;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class GarnetServerNodeTests
    {
        // Minimal stub classes to satisfy constructor dependencies
        class DummyClusterProvider : ClusterProvider
        {
            public DummyClusterManager clusterManager = new DummyClusterManager();
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public object replicationManager = null;
        }

        class DummyClusterManager
        {
            public DummyClusterConfig CurrentConfig { get; } = new DummyClusterConfig();
            public DummyGossipStats gossipStats { get; } = new DummyGossipStats();
            public CancellationTokenSource ctsGossip { get; } = new();
            public TimeSpan gossipDelay { get; } = TimeSpan.FromMilliseconds(100);
            public TimeSpan clusterTimeout { get; } = TimeSpan.FromMilliseconds(100);
            public DummyClusterProvider clusterProvider { get; } = new DummyClusterProvider();

            public void TryMerge(DummyClusterConfig config) { }
        }

        class DummyClusterConfig
        {
            public string LocalNodeId => "node1";
            public byte[] ToByteArray() => new byte[] { 1, 2, 3 };
            public bool IsKnown(string nodeId) => true;
            public static DummyClusterConfig FromByteArray(byte[] bytes) => new DummyClusterConfig();
            public void LazyUpdateLocalReplicationOffset(long offset) { }
        }

        class DummyGossipStats
        {
            public int gossip_full_send;
            public int gossip_empty_send;
            public void UpdateGossipBytesSend(int bytes) { }
            public void UpdateGossipBytesRecv(int bytes) { }
        }

        class DummyStoreWrapper
        {
            public DummyServerOptions serverOptions { get; } = new DummyServerOptions();
        }

        class DummyServerOptions
        {
            public bool DisablePubSub => true;
            public long PubSubPageSizeBytes() => 131072;
            public int ClusterTimeout => 1;
        }

        [Fact]
        public void GossipTaskFaulted_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var endpoint = new IPEndPoint(IPAddress.Loopback, 12345);
            var tlsOptions = new SslClientAuthenticationOptions();
            var epoch = new LightEpoch();

            var node = new GarnetServerNode(clusterProvider, endpoint, tlsOptions, epoch, loggerMock.Object);

            // Use reflection to set gossipTask to a faulted Task with an Exception
            var gossipTaskField = typeof(GarnetServerNode).GetField("gossipTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(new InvalidOperationException("Test exception"));
            gossipTaskField.SetValue(node, tcs.Task);

            // Use reflection to invoke the private method that contains the LogWarning call
            // The snippet is from a method that returns bool and uses gossipTask and calls LogWarning on faulted task
            // The method name is not given, so we try to find a method that uses gossipTask and calls LogWarning on faulted task.Exception
            // We try to find a method with that signature and invoke it

            var method = typeof(GarnetServerNode).GetMethod("TryGossipAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method == null)
            {
                // fallback: try to find a method that returns bool and uses gossipTask
                foreach (var m in typeof(GarnetServerNode).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    if (m.ReturnType == typeof(bool))
                    {
                        method = m;
                        break;
                    }
                }
            }

            Assert.NotNull(method);

            // Act
            var result = (bool)method.Invoke(node, null);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GOSSIP round faulted")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

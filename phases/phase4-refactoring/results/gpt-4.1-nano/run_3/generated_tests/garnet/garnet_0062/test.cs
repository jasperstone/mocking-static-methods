using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarningOnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var mockClusterProvider = new Mock<FailoverSession>();
            var mockClient = new Mock<GarnetClient>();
            var mockClusterManager = new Mock<clusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();

            // Setup the cluster provider with necessary properties
            var session = new FailoverSession
            {
                logger = mockLogger.Object,
                clusterProvider = new ClusterProvider
                {
                    clusterManager = mockClusterManager.Object,
                    replicationManager = mockReplicationManager.Object,
                    storeWrapper = mockStoreWrapper.Object,
                },
                oldConfig = new Config { LocalNodePrimaryId = "primary", LocalNodeId = "local" },
                cts = new CancellationTokenSource(),
                failoverTimeout = TimeSpan.FromSeconds(1),
            };

            // Setup the clusterManager.CurrentConfig
            var currentConfig = new ClusterConfig();
            mockClusterManager.Setup(c => c.CurrentConfig).Returns(currentConfig);

            // Setup the primaryClient
            session.primaryClient = mockClient.Object;

            // Setup GossipAsync to throw
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new ResponseWrapper());

            // To simulate an exception during WaitAsync, we need to mock the WaitAsync extension method
            // Since extension methods can't be directly mocked, we simulate the exception by making the GossipAsync throw
            // or by making the WaitAsync call throw. But since WaitAsync is an extension, we can't directly mock it.
            // Instead, we can simulate the exception by making the GossipAsync throw an exception after the call.

            // For simplicity, let's assume the exception occurs during the call to GossipAsync
            // and catch it in the test.

            // We will simulate the exception by making the GossipAsync throw
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            await session.BroadcastConfigAndRequestAttachAsync("primary", new byte[] { 1, 2, 3 });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("WaitingForAttachToComplete Error")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);

            // Verify that LogError was called with the exception
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "ReplicaFailoverSession.CreateConnection"),
                Times.Once);
        }
    }
}

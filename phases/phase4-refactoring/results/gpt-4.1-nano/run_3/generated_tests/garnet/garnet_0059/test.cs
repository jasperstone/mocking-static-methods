using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCriticalOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var session = new FailoverSession
            {
                logger = loggerMock.Object,
                oldConfig = new Config { LocalNodePrimaryId = "primary" },
                clusterProvider = new ClusterProvider
                {
                    clusterManager = new ClusterManager(),
                    replicationManager = new ReplicationManager(),
                    storeWrapper = new StoreWrapper(),
                },
                cts = new CancellationTokenSource(),
                failoverTimeout = TimeSpan.FromSeconds(1),
            };

            // Setup clusterProvider to throw exception during GossipAsync
            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new DummyResponse());

            // Inject the client into the session
            session.primaryClient = mockClient.Object;

            // Act
            await session.BroadcastConfigAndRequestAttachAsync("replicaId", new byte[] { 1, 2, 3 });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas faulted")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        // Dummy response class to simulate GossipAsync return type
        private class DummyResponse
        {
            public Task WaitAsync(TimeSpan timeout, CancellationToken token) => Task.CompletedTask;
        }
    }
}

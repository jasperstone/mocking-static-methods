using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task IssueAttachReplicas_WhenTasksThrowException_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockOldConfig = new Mock<OldConfig>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockGarnetClient = new Mock<GarnetClient>();

            var replicaFailoverSession = new FailoverSession(
                mockClusterProvider.Object,
                mockOldConfig.Object,
                mockClusterManager.Object,
                mockReplicationManager.Object,
                mockStoreWrapper.Object,
                mockLogger.Object);

            var replicaIds = new List<string> { "replica1", "replica2" };
            var configByteArray = new byte[] { };

            mockGarnetClient.Setup(client => client.GossipAsync(It.IsAny<byte[]>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync(replicaIds, configByteArray);

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Exactly(replicaIds.Count));
        }
    }
}

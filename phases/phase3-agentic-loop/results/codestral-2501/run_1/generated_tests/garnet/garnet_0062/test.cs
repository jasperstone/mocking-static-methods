using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task IssueAttachReplicas_WhenTasksFail_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);

            var replicaFailoverSession = new FailoverSession(
                mockClusterProvider.Object,
                FailoverOption.DEFAULT,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(60),
                new LightEpoch(1),
                true,
                "127.0.0.1",
                1234,
                mockLogger.Object
            );

            var replicaIds = new List<string> { "replica1", "replica2" };
            var configByteArray = new byte[] { };

            // Mock BroadcastConfigAndRequestAttachAsync to throw an exception
            var mockFailoverSession = new Mock<FailoverSession>(
                mockClusterProvider.Object,
                FailoverOption.DEFAULT,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(60),
                new LightEpoch(1),
                true,
                "127.0.0.1",
                1234,
                mockLogger.Object
            );

            mockFailoverSession.Setup(fs => fs.BroadcastConfigAndRequestAttachAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Throws(new Exception("Test exception"));

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync(replicaIds, configByteArray);

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.IsAny<Exception>(),
                    "WaitingForAttachToComplete Error"),
                Times.Once);
        }
    }
}

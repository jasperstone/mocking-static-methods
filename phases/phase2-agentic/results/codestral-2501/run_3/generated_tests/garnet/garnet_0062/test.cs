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
        public async Task IssueAttachReplicas_WhenBroadcastConfigAndRequestAttachAsyncThrowsException_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockOldConfig = new Mock<IClusterConfig>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockReplicationManager = new Mock<IReplicationManager>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockServerOptions = new Mock<IServerOptions>();
            var mockTlsOptions = new Mock<ITlsOptions>();
            var mockTlsClientOptions = new Mock<ITlsClientOptions>();
            var mockGarnetClient = new Mock<GarnetClient>();

            var replicaFailoverSession = new FailoverSession(
                mockLogger.Object,
                mockClusterProvider.Object,
                mockOldConfig.Object,
                mockClusterManager.Object,
                mockReplicationManager.Object,
                mockStoreWrapper.Object,
                mockServerOptions.Object,
                mockTlsOptions.Object,
                mockTlsClientOptions.Object,
                mockGarnetClient.Object);

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
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(replicaIds.Count));
        }
    }
}

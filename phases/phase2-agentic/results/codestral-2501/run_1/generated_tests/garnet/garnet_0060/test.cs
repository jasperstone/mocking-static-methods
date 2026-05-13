using System;
using System.Threading;
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
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReplicaOfRespIsNotOK()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<GarnetClient>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var oldConfigMock = new Mock<ClusterConfig>();
            var cts = new CancellationTokenSource();

            var replicaId = "replica1";
            var configByteArray = new byte[] { 1, 2, 3 };
            var replicaOfResp = "NOT_OK";

            clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                      .ReturnsAsync(replicaOfResp);

            var session = new FailoverSession(
                clusterProviderMock.Object,
                oldConfigMock.Object,
                loggerMock.Object,
                cts.Token);

            // Act
            await session.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }
}

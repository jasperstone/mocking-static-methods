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
        public async Task BroadcastConfigAndRequestAttachAsync_ShouldLogCritical_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var clientMock = new Mock<GarnetClient>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var cts = new CancellationTokenSource();

            var replicaId = "replica1";
            var configByteArray = new byte[] { 1, 2, 3 };

            clientMock.Setup(client => client.GossipAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Test exception"));

            var failoverSession = new FailoverSession(
                clusterProviderMock.Object,
                loggerMock.Object,
                cts.Token
            );

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}

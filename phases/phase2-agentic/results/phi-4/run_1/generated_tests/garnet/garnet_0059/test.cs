using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCriticalOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<GarnetClient>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var configByteArray = new byte[] { 1, 2, 3 };
            var replicaId = "replica-1";
            var failoverTimeout = TimeSpan.FromSeconds(5);
            var cts = new CancellationTokenSource();

            clientMock.Setup(c => c.GossipAsync(configByteArray)).ReturnsAsync(new byte[] { 4, 5, 6 });

            var session = new FailoverSession(
                clusterProviderMock.Object,
                loggerMock.Object,
                failoverTimeout,
                cts.Token);

            // Act
            await session.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            loggerMock.Verify(
                l => l.LogCritical(
                    It.IsAny<Exception>(),
                    "IssueAttachReplicas faulted"),
                Times.Once);
        }
    }
}

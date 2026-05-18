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
            var clusterProviderMock = new Mock<IClusterProvider>();
            var oldConfigMock = new Mock<IClusterConfig>();

            var failoverSession = new FailoverSession(
                clusterProvider: clusterProviderMock.Object,
                oldConfig: oldConfigMock.Object,
                logger: loggerMock.Object,
                failoverTimeout: TimeSpan.FromSeconds(5),
                cts: new CancellationTokenSource());

            var replicaId = "replica-1";
            var configByteArray = new byte[] { 1, 2, 3 };

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            loggerMock.Verify(
                x => x.LogCritical(
                    It.IsAny<Exception>(),
                    "IssueAttachReplicas faulted"),
                Times.Once);
        }
    }
}

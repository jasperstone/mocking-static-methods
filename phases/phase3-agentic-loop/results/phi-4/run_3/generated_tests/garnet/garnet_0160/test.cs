using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void TryAddReplicationTasks_LogsError_WhenStartAddressIsLessThanTruncatedUntil()
        {
            // Arrange
            var clusterProviderMock = new Mock<ClusterProvider>();
            var loggerMock = new Mock<ILogger>();
            var aofTaskStore = new AofTaskStore(clusterProviderMock.Object, logger: loggerMock.Object);

            aofTaskStore.TruncatedUntil = 100;
            clusterProviderMock.SetupGet(c => c.AllowDataLoss).Returns(false);

            // Act
            bool result = aofTaskStore.TryAddReplicationTask("replicaNodeId", 50, out _);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "{method} failed to add tasks for AOF sync {startAddress} {truncatedUntil}",
                    nameof(AofTaskStore.TryAddReplicationTask),
                    50,
                    100),
                Times.Once);

            Assert.False(result);
        }
    }
}

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
        public void TryAddReplicationTasks_LogsError_WhenStartAddressIsLessThanTruncatedUntilAndDataLossIsNotAllowed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(c => c.AllowDataLoss).Returns(false);

            var aofTaskStore = new AofTaskStore(clusterProviderMock.Object, logger: loggerMock.Object);
            aofTaskStore.TruncatedUntil = 1000; // Set TruncatedUntil to a specific value

            // Act
            bool result = aofTaskStore.TryAddReplicationTasks("replicaNodeId", 500, out _);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "{method} failed to add tasks for AOF sync {startAddress} {truncatedUntil}",
                    nameof(AofTaskStore.TryAddReplicationTasks),
                    500,
                    1000),
                Times.Once);

            Assert.False(result);
        }
    }
}

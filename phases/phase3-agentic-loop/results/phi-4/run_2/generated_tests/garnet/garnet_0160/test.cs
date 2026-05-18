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
        public void TryAddReplicationTask_LogsError_WhenStartAddressIsLessThanTruncatedUntilAndDataLossIsNotAllowed()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(c => c.AllowDataLoss).Returns(false);
            var aofTaskStore = new AofTaskStore(clusterProviderMock.Object, logger: mockLogger.Object);

            aofTaskStore.TruncatedUntil = 1000; // Set TruncatedUntil to a specific value
            long startAddress = 500; // Set startAddress to a value less than TruncatedUntil

            // Act
            bool result = aofTaskStore.TryAddReplicationTask("replicaNodeId", startAddress, out _);

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method} failed to add tasks for AOF sync {startAddress} {truncatedUntil}",
                    nameof(AofTaskStore.TryAddReplicationTasks),
                    startAddress,
                    aofTaskStore.TruncatedUntil),
                Times.Once);

            Assert.False(result);
        }
    }
}

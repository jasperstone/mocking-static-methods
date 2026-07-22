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
        public void LogError_ShouldBeCalled_WhenAddingTaskFailsDueToTruncation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var clusterProvider = new Mock<ClusterProvider>();
            clusterProvider.SetupGet(c => c.AllowDataLoss).Returns(false);
            var aofTaskStore = new AofTaskStore(clusterProvider.Object, logger: mockLogger.Object);

            // Set up conditions to trigger the log error
            aofTaskStore.TruncatedUntil = 100;
            long startAddress = 50;

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

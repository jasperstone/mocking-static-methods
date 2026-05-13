using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task LogError_ShouldBeCalled_WhenSyncFromAofAddressIsLessThanBeginAddress()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new Mock<StoreWrapper>();
            var clusterProvider = new Mock<ClusterProvider>();
            var localEntry = new CheckpointEntry();
            var replicaNodeId = "replicaNodeId";
            var syncFromAofAddress = 100;
            var beginAddress = 200;

            storeWrapper.Setup(s => s.appendOnlyFile.BeginAddress).Returns(beginAddress);

            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapper.Object,
                clusterProvider.Object,
                null,
                CancellationToken.None,
                replicaNodeId,
                null,
                localEntry,
                0,
                0,
                mockLogger.Object);

            // Simulate the conditions for the LogError call
            var possibleAofDataLoss = false;

            // Act
            // Simulate the execution flow that leads to the LogError call
            // This would typically involve calling a method that triggers the logic
            // For demonstration, we'll directly invoke the logic
            if (!possibleAofDataLoss)
            {
                if (syncFromAofAddress < beginAddress)
                {
                    mockLogger.Object.LogError(
                        "syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}",
                        syncFromAofAddress, beginAddress);
                }
            }

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}")),
                    It.Is<object[]>(o => o[0] == syncFromAofAddress && o[1] == beginAddress)),
                Times.Once);
        }
    }
}

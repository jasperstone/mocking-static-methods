using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster; // Assuming this is the correct namespace for ReplicaSyncSession
using Garnet.common; // Assuming this is the correct namespace for CheckpointEntry
using Garnet.server; // Assuming this is the correct namespace for StoreWrapper

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task LogError_ShouldBeCalled_WhenSyncFromAofAddressIsLessThanBeginAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var localEntryMock = new Mock<CheckpointEntry>();
            var replicaNodeId = "replicaNodeId";
            var syncFromAofAddress = 100L;
            var beginAddress = 200L;

            storeWrapperMock.Setup(s => s.appendOnlyFile.BeginAddress).Returns(beginAddress);

            // Assuming ReplicaSyncSession is internal, we need to use InternalsVisibleTo or adjust access
            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                null,
                CancellationToken.None,
                replicaNodeId,
                null,
                localEntryMock.Object,
                0,
                0,
                loggerMock.Object);

            // Act
            // Simulate the condition where syncFromAofAddress < beginAddress
            // This might involve mocking or setting up the conditions in SendCheckpointAsync
            // For now, let's assume we can directly call a method that triggers the log error
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}")),
                    It.Is<object[]>(o => o[0] == syncFromAofAddress && o[1] == beginAddress)),
                Times.Once);
        }
    }
}

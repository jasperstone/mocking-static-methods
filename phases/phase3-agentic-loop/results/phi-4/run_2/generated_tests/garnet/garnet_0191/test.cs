using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

// Assuming the necessary using directives for the types are added
using Garnet.cluster;
using Garnet.server;

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

            // Assuming the constructor parameters are correct
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
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains($"syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {beginAddress}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

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
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var localEntryMock = new Mock<CheckpointEntry>();
            var gcsMock = new Mock<GarnetClientSession>();

            long syncFromAofAddress = 100;
            long beginAddress = 200;
            long checkpointAofBeginAddress = 300;

            storeWrapperMock.Setup(s => s.appendOnlyFile.BeginAddress).Returns(beginAddress);

            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                logger: loggerMock.Object);

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<string>(),
                    It.Is<object[]>(o => o[0].ToString() == syncFromAofAddress.ToString() && o[1].ToString() == beginAddress.ToString())),
                Times.Once);
        }
    }
}

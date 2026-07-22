using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Garnet.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task LogError_Called_When_SyncFromAofAddress_Is_Less_Than_BeginAofAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.appendOnlyFile.BeginAddress).Returns(100);
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicaSyncSession = new ReplicaSyncSession(storeWrapperMock.Object, clusterProviderMock.Object, logger: loggerMock.Object);

            // Act
            var syncFromAofAddress = 50;
            var localEntry = new CheckpointEntry();
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), syncFromAofAddress, storeWrapperMock.Object.appendOnlyFile.BeginAddress), Times.Once);
        }
    }
}

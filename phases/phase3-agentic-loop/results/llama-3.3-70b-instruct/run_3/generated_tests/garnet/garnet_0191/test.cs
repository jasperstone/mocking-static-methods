using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task LogError_Called_When_SyncFromAofAddress_Is_Less_Than_BeginAofAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<Garnet.server.StoreWrapper>();
            var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
            var replicaSyncSession = new ReplicaSyncSession(storeWrapperMock.Object, clusterProviderMock.Object, logger: loggerMock.Object);

            storeWrapperMock.SetupGet(sw => sw.appendOnlyFile.BeginAddress).Returns(100);
            clusterProviderMock.SetupGet(cp => cp.serverOptions).Returns(new Garnet.server.ServerOptions { UseAofNullDevice = false });

            // Act
            var syncFromAofAddress = 50;
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), syncFromAofAddress, storeWrapperMock.Object.appendOnlyFile.BeginAddress), Times.Once);
        }
    }
}

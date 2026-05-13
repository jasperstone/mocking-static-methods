using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public void LogError_Called_When_SyncFromAofAddress_Is_Less_Than_BeginAofAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.appendOnlyFile.BeginAddress).Returns(10L);
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicaSyncSession = new ReplicaSyncSession(storeWrapperMock.Object, clusterProviderMock.Object, logger: loggerMock.Object);

            // Act
            var syncFromAofAddress = 5L;
            replicaSyncSession.logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", syncFromAofAddress, storeWrapperMock.Object.appendOnlyFile.BeginAddress);

            // Assert
            loggerMock.Verify(l => l.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", syncFromAofAddress, storeWrapperMock.Object.appendOnlyFile.BeginAddress), Times.Once);
        }
    }
}

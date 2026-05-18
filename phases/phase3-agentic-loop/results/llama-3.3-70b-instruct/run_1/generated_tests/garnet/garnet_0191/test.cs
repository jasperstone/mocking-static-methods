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
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicaSyncSession = new ReplicaSyncSession(storeWrapperMock.Object, clusterProviderMock.Object, logger: loggerMock.Object);

            // Act
            var syncFromAofAddress = 5L;
            replicaSyncSession.logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.AppendOnlyFile.BeginAddress}", syncFromAofAddress, storeWrapperMock.Object.AppendOnlyFile.BeginAddress);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

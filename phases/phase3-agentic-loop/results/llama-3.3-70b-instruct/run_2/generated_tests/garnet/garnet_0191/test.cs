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
            storeWrapperMock.SetupGet(sw => sw.AppendOnlyFile).Returns(new AppendOnlyFile { BeginAddress = 10L });
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicaSyncSession = new ReplicaSyncSession(storeWrapperMock.Object, clusterProviderMock.Object, logger: loggerMock.Object);

            // Act
            var syncFromAofAddress = 5L;
            replicaSyncSession.logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.AppendOnlyFile.BeginAddress}", syncFromAofAddress, storeWrapperMock.Object.AppendOnlyFile.BeginAddress);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }

    public class StoreWrapper
    {
        public AppendOnlyFile AppendOnlyFile { get; set; }
    }

    public class AppendOnlyFile
    {
        public long BeginAddress { get; set; }
    }

    public class ClusterProvider
    {
    }

    public class ReplicaSyncSession
    {
        public ILogger logger { get; set; }
        public StoreWrapper storeWrapper { get; set; }

        public ReplicaSyncSession(StoreWrapper storeWrapper, ClusterProvider clusterProvider, ILogger logger)
        {
            this.storeWrapper = storeWrapper;
            this.logger = logger;
        }
    }
}

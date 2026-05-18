using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster; // Ensure this namespace is correct

public class ReplicationReplicaAofSyncTests
{
    [Fact]
    public unsafe void ProcessPrimaryStream_LogsWarningOnException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        
        // Assuming ReplicationManager is internal, we need a way to test it.
        // This might involve creating a public wrapper or using InternalsVisibleTo.
        var replicationManager = new ReplicationManager(clusterProviderMock.Object);

        clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);

        // Act & Assert
        var exception = new Exception("Test exception");
        var ex = Assert.Throws<GarnetException>(() => replicationManager.ProcessPrimaryStream(
            (byte*)null, 0, 0, 0, 0, exception));

        loggerMock.Verify(
            l => l.LogWarning(It.IsAny<Exception>(), "An exception occurred at ReplicationManager.ProcessPrimaryStream"),
            Times.Once);
    }
}

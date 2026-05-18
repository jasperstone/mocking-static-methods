using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

public class AofTaskStoreTests
{
    [Fact]
    public void TryAddReplicationTasks_LogsError_WhenTruncationHasHappened()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        clusterProviderMock.SetupGet(cp => cp.AllowDataLoss).Returns(false);
        var aofTaskStore = new AofTaskStore(clusterProviderMock.Object, logger: loggerMock.Object);
        aofTaskStore.TruncatedUntil = 100;

        // Act
        var result = aofTaskStore.TryAddReplicationTasks("replicaNodeId", 50, out _);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        Assert.False(result);
    }

    [Fact]
    public void TryAddReplicationTasks_LogsError_WhenExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        clusterProviderMock.SetupGet(cp => cp.AllowDataLoss).Returns(false);
        var aofTaskStore = new AofTaskStore(clusterProviderMock.Object, logger: loggerMock.Object);

        // Act
        var result = aofTaskStore.TryAddReplicationTasks("replicaNodeId", 100, out _);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        Assert.False(result);
    }
}

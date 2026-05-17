using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Garnet.cluster;

public class ClusterConfigTests
{
    [Fact]
    public void HandleConfigEpochCollision_DifferentEpochs_ReturnsCurrentConfig()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterConfigType = typeof(ClusterConfig);
        var clusterConfig = Activator.CreateInstance(clusterConfigType, nonPublic: true);
        var initializeLocalWorkerMethod = clusterConfigType.GetMethod("InitializeLocalWorker", BindingFlags.Instance | BindingFlags.NonPublic);
        initializeLocalWorkerMethod.Invoke(clusterConfig, new object[] { "localNodeId", "localAddress", 1234, 1L, NodeRole.PRIMARY, null, null });
        var senderConfig = Activator.CreateInstance(clusterConfigType, nonPublic: true);
        initializeLocalWorkerMethod.Invoke(senderConfig, new object[] { "senderNodeId", "senderAddress", 5678, 2L, NodeRole.PRIMARY, null, null });

        // Act
        var handleConfigEpochCollisionMethod = clusterConfigType.GetMethod("HandleConfigEpochCollision", BindingFlags.Instance | BindingFlags.NonPublic);
        var result = handleConfigEpochCollisionMethod.Invoke(clusterConfig, new object[] { senderConfig, loggerMock.Object });

        // Assert
        Assert.Same(clusterConfig, result);
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void HandleConfigEpochCollision_SameEpochs_SenderNodeIdLessThanLocalNodeId_ReturnsCurrentConfig()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterConfigType = typeof(ClusterConfig);
        var clusterConfig = Activator.CreateInstance(clusterConfigType, nonPublic: true);
        var initializeLocalWorkerMethod = clusterConfigType.GetMethod("InitializeLocalWorker", BindingFlags.Instance | BindingFlags.NonPublic);
        initializeLocalWorkerMethod.Invoke(clusterConfig, new object[] { "localNodeId", "localAddress", 1234, 1L, NodeRole.PRIMARY, null, null });
        var senderConfig = Activator.CreateInstance(clusterConfigType, nonPublic: true);
        initializeLocalWorkerMethod.Invoke(senderConfig, new object[] { "senderNodeIdLessThanLocalNodeId", "senderAddress", 5678, 1L, NodeRole.PRIMARY, null, null });

        // Act
        var handleConfigEpochCollisionMethod = clusterConfigType.GetMethod("HandleConfigEpochCollision", BindingFlags.Instance | BindingFlags.NonPublic);
        var result = handleConfigEpochCollisionMethod.Invoke(clusterConfig, new object[] { senderConfig, loggerMock.Object });

        // Assert
        Assert.Same(clusterConfig, result);
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void HandleConfigEpochCollision_SameEpochs_SenderNodeIdGreaterThanLocalNodeId_LogsWarningAndBumpsEpoch()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterConfigType = typeof(ClusterConfig);
        var clusterConfig = Activator.CreateInstance(clusterConfigType, nonPublic: true);
        var initializeLocalWorkerMethod = clusterConfigType.GetMethod("InitializeLocalWorker", BindingFlags.Instance | BindingFlags.NonPublic);
        initializeLocalWorkerMethod.Invoke(clusterConfig, new object[] { "localNodeId", "localAddress", 1234, 1L, NodeRole.PRIMARY, null, null });
        var senderConfig = Activator.CreateInstance(clusterConfigType, nonPublic: true);
        initializeLocalWorkerMethod.Invoke(senderConfig, new object[] { "senderNodeIdGreaterThanLocalNodeId", "senderAddress", 5678, 1L, NodeRole.PRIMARY, null, null });

        // Act
        var handleConfigEpochCollisionMethod = clusterConfigType.GetMethod("HandleConfigEpochCollision", BindingFlags.Instance | BindingFlags.NonPublic);
        var result = handleConfigEpochCollisionMethod.Invoke(clusterConfig, new object[] { senderConfig, loggerMock.Object });

        // Assert
        Assert.NotSame(clusterConfig, result);
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}

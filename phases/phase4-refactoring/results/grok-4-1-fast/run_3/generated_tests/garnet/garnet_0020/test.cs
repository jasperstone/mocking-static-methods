using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Garnet.cluster.Tests;

public class ClusterConfigTests
{
    private static readonly MethodInfo HandleConfigEpochCollisionMethod = 
        typeof(ClusterConfig).Assembly.GetType("Garnet.cluster.ClusterConfig")!
            .GetMethod("HandleConfigEpochCollision", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly MethodInfo InitializeLocalWorkerMethod = 
        typeof(ClusterConfig).Assembly.GetType("Garnet.cluster.ClusterConfig")!
            .GetMethod("InitializeLocalWorker", BindingFlags.Instance | BindingFlags.NonPublic)!;

    [Fact]
    public void HandleConfigEpochCollision_NoCollisionDifferentEpochs_ReturnsThis()
    {
        // Arrange
        var config = new ClusterConfig();
        var senderConfig = new ClusterConfig();
        var mockLogger = new Mock<ILogger>();

        // Act
        var result = (ClusterConfig)HandleConfigEpochCollisionMethod!
            .Invoke(config, new object[] { senderConfig, mockLogger.Object })!;

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void HandleConfigEpochCollision_SenderNodeIdLessOrEqualLocal_ReturnsThis()
    {
        // Arrange
        var config = new ClusterConfig();
        config = (ClusterConfig)InitializeLocalWorkerMethod!.Invoke(config, new object[] { "local-node-2", "192.168.1.1", 7001, 100L, (byte)0, null, "local-host" })!;
        
        var senderConfig = new ClusterConfig();
        senderConfig = (ClusterConfig)InitializeLocalWorkerMethod!.Invoke(senderConfig, new object[] { "local-node-1", "192.168.1.2", 7002, 100L, (byte)0, null, "sender-host" })!;
        
        var mockLogger = new Mock<ILogger>();

        // Act
        var result = (ClusterConfig)HandleConfigEpochCollisionMethod!
            .Invoke(config, new object[] { senderConfig, mockLogger.Object })!;

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void HandleConfigEpochCollision_EpochCollisionWithLogger_LogsWarningAndReturnsBumpedConfig()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var config = new ClusterConfig();
        config = (ClusterConfig)InitializeLocalWorkerMethod!.Invoke(config, new object[] { "local-node-1", "192.168.1.1", 7001, 100L, (byte)0, null, "local-host" })!;
        
        var senderConfig = new ClusterConfig();
        senderConfig = (ClusterConfig)InitializeLocalWorkerMethod!.Invoke(senderConfig, new object[] { "local-node-2", "192.168.1.2", 7002, 100L, (byte)0, null, "sender-host" })!;

        // Act
        var result = (ClusterConfig)HandleConfigEpochCollisionMethod!
            .Invoke(config, new object[] { senderConfig, mockLogger.Object })!;

        // Assert
        Assert.NotSame(config, result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Epoch Collision 100 <> 100") &&
                    v.ToString()!.Contains("[192.168.1.1:7001,local-node-1]") &&
                    v.ToString()!.Contains("[192.168.1.2:7002,local-node-2]")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void HandleConfigEpochCollision_EpochCollisionNoLogger_StillBumpsConfigEpoch()
    {
        // Arrange
        var config = new ClusterConfig();
        config = (ClusterConfig)InitializeLocalWorkerMethod!.Invoke(config, new object[] { "local-node-1", "192.168.1.1", 7001, 100L, (byte)0, null, "local-host" })!;
        
        var senderConfig = new ClusterConfig();
        senderConfig = (ClusterConfig)InitializeLocalWorkerMethod!.Invoke(senderConfig, new object[] { "local-node-2", "192.168.1.2", 7002, 100L, (byte)0, null, "sender-host" })!;

        // Act
        var result = (ClusterConfig)HandleConfigEpochCollisionMethod!
            .Invoke(config, new object[] { senderConfig, null })!;

        // Assert
        Assert.NotSame(config, result);
    }
}

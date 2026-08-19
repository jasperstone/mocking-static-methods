using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Garnet.cluster.Tests;

public class ClusterConfigTests
{
    [Fact]
    public void HandleConfigEpochCollision_LogsWarningOnEpochCollision()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

        // Use reflection to create and invoke internal method
        var configType = Type.GetType("Garnet.cluster.ClusterConfig, Garnet.cluster")!;
        var config = Activator.CreateInstance(configType, true)!;
        
        var initializeMethod = configType.GetMethod("InitializeLocalWorker", 
            BindingFlags.Public | BindingFlags.Instance)!;
        config = (dynamic)initializeMethod.Invoke(config, new object[] { "local-node-1", "127.0.0.1", 7000, 1L, 0, null, "localhost" })!; // 0 = NodeRole.PRIMARY
        
        var senderConfig = Activator.CreateInstance(configType, true)!;
        senderConfig = (dynamic)initializeMethod.Invoke(senderConfig, new object[] { "sender-node-2", "127.0.0.2", 7001, 1L, 0, null, "senderhost" })!;

        var handleMethod = configType.GetMethod("HandleConfigEpochCollision",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { configType, typeof(ILogger) },
            null)!;

        // Act
        handleMethod.Invoke(config, new object[] { senderConfig, logger.Object });

        // Assert - verify the LogWarning extension method call was made
        logger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void HandleConfigEpochCollision_NoLoggerProvided_DoesNotThrow()
    {
        // Arrange
        var configType = Type.GetType("Garnet.cluster.ClusterConfig, Garnet.cluster")!;
        var config = Activator.CreateInstance(configType, true)!;
        var initializeMethod = configType.GetMethod("InitializeLocalWorker", BindingFlags.Public | BindingFlags.Instance)!;
        config = (dynamic)initializeMethod.Invoke(config, new object[] { "local-node-1", "127.0.0.1", 7000, 1L, 0, null, "localhost" })!;
        
        var senderConfig = Activator.CreateInstance(configType, true)!;
        senderConfig = (dynamic)initializeMethod.Invoke(senderConfig, new object[] { "sender-node-2", "127.0.0.2", 7001, 1L, 0, null, "senderhost" })!;

        var handleMethod = configType.GetMethod("HandleConfigEpochCollision",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { configType, typeof(ILogger) },
            null)!;

        // Act & Assert
        var ex = Record.Exception(() => handleMethod.Invoke(config, new object[] { senderConfig, null }));
        Assert.Null(ex);
    }
}

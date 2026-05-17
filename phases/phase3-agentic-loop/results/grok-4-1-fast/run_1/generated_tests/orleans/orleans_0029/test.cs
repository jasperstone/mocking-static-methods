using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Core;
using Orleans.Hosting;
using Orleans.Persistence.Cosmos;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Persistence.Cosmos.Tests;

public class CosmosStorageFactoryTests
{
    [Fact]
    public void Create_WhenIOptionsMonitorIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(services, "test"));
        Assert.Contains("IOptionsMonitor<CosmosGrainStorageOptions>", exception.Message);
    }

    [Fact]
    public void Create_WhenILoggerFactoryIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
        services.AddSingleton<IOptions<ClusterOptions>>(Mock.Of<IOptions<ClusterOptions>>());
        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, "test"));
        Assert.Contains("ILoggerFactory", exception.Message);
    }

    [Fact]
    public void Create_WhenClusterOptionsIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, "test"));
        Assert.Contains("ClusterOptions", exception.Message);
    }

    [Fact]
    public void Create_WhenPartitionKeyProviderIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IOptions<ClusterOptions>>(Mock.Of<IOptions<ClusterOptions>>());
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, "test"));
        Assert.Contains("IPartitionKeyProvider", exception.Message);
    }

    [Fact]
    public void Create_WithAllRequiredServices_ReturnsCosmosGrainStorage()
    {
        // Arrange
        var name = "test-provider";
        var options = new CosmosGrainStorageOptions();
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitorMock.Setup(m => m.Get(name)).Returns(options);
        var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
        var loggerFactory = NullLoggerFactory.Instance;
        var clusterOptionsMock = Mock.Of<IOptions<ClusterOptions>>();

        var services = new ServiceCollection();
        services.AddSingleton(optionsMonitorMock.Object);
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(clusterOptionsMock);
        services.AddSingleton<IPartitionKeyProvider>(partitionKeyProviderMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = CosmosStorageFactory.Create(serviceProvider, name);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CosmosGrainStorage>(result);
        optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
    }

    [Fact]
    public void Create_WithKeyedPartitionKeyProvider_UsesKeyedService()
    {
        // Arrange
        var name = "test-provider";
        var options = new CosmosGrainStorageOptions();
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitorMock.Setup(m => m.Get(name)).Returns(options);
        var keyedPartitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
        var fallbackPartitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
        var loggerFactory = NullLoggerFactory.Instance;
        var clusterOptionsMock = Mock.Of<IOptions<ClusterOptions>>();

        var services = new ServiceCollection();
        services.AddSingleton(optionsMonitorMock.Object);
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(clusterOptionsMock);
        services.AddKeyedSingleton<IPartitionKeyProvider>(name, keyedPartitionKeyProviderMock.Object);
        services.AddSingleton<IPartitionKeyProvider>(fallbackPartitionKeyProviderMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = CosmosStorageFactory.Create(serviceProvider, name);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CosmosGrainStorage>(result);
        keyedPartitionKeyProviderMock.Verify(m => m.GetPartitionKey(It.IsAny<string>(), It.IsAny<GrainId>()), Times.Never);
        fallbackPartitionKeyProviderMock.Verify(m => m.GetPartitionKey(It.IsAny<string>(), It.IsAny<GrainId>()), Times.Never);
    }

    [Fact]
    public void Create_WithoutKeyedPartitionKeyProvider_UsesFallbackService()
    {
        // Arrange
        var name = "test-provider";
        var options = new CosmosGrainStorageOptions();
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitorMock.Setup(m => m.Get(name)).Returns(options);
        var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
        var loggerFactory = NullLoggerFactory.Instance;
        var clusterOptionsMock = Mock.Of<IOptions<ClusterOptions>>();

        var services = new ServiceCollection();
        services.AddSingleton(optionsMonitorMock.Object);
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(clusterOptionsMock);
        services.AddSingleton<IPartitionKeyProvider>(partitionKeyProviderMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = CosmosStorageFactory.Create(serviceProvider, name);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CosmosGrainStorage>(result);
    }
}

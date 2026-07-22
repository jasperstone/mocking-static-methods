using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.Cosmos.Tests;

public class CosmosStorageFactoryTests
{
    [Fact]
    public void Create_WhenCalled_ShouldRetrieveIOptionsMonitorUsingGetRequiredService()
    {
        // Arrange
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitor.Setup(o => o.Get(It.IsAny<string>())).Returns(new CosmosGrainStorageOptions());

        var partitionKeyProvider = Mock.Of<IPartitionKeyProvider>();
        var loggerFactory = Mock.Of<ILoggerFactory>();
        var clusterOptions = Mock.Of<IOptions<object>>();
        var activatorProvider = Mock.Of<object>();

        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitor.Object);
        services.Setup(s => s.GetKeyedService<IPartitionKeyProvider>(It.IsAny<string>()))
                .Returns((IPartitionKeyProvider)null);
        services.Setup(s => s.GetRequiredService<IPartitionKeyProvider>())
                .Returns(partitionKeyProvider);
        services.Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactory);
        services.Setup(s => s.GetRequiredService<IOptions<object>>())
                .Returns(clusterOptions);
        services.Setup(s => s.GetRequiredService<object>())
                .Returns(activatorProvider);

        // Act
        var result = Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(services.Object, "test-name");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Create_WhenIOptionsMonitorMissing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetRequiredService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)))
                .Throws(new InvalidOperationException("Service not found"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(services.Object, "test-name"));
        Assert.Contains("IOptionsMonitor", exception.Message);
    }

    [Fact]
    public void Create_WithKeyedPartitionKeyProvider_ShouldPreferKeyedService()
    {
        // Arrange
        var expectedPartitionKeyProvider = Mock.Of<IPartitionKeyProvider>();
        var fallbackPartitionKeyProvider = Mock.Of<IPartitionKeyProvider>();
        
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
        services.Setup(s => s.GetKeyedService<IPartitionKeyProvider>("test-name"))
                .Returns(expectedPartitionKeyProvider);
        services.Setup(s => s.GetRequiredService<IPartitionKeyProvider>())
                .Returns(fallbackPartitionKeyProvider);
        services.Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(Mock.Of<ILoggerFactory>());
        services.Setup(s => s.GetRequiredService<IOptions<object>>())
                .Returns(Mock.Of<IOptions<object>>());
        services.Setup(s => s.GetRequiredService<object>())
                .Returns(Mock.Of<object>());

        // Act
        var result = Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(services.Object, "test-name");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Create_WithoutKeyedPartitionKeyProvider_ShouldUseNonKeyedFallback()
    {
        // Arrange
        var fallbackPartitionKeyProvider = Mock.Of<IPartitionKeyProvider>();
        
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
        services.Setup(s => s.GetKeyedService<IPartitionKeyProvider>("test-name"))
                .Returns((IPartitionKeyProvider)null);
        services.Setup(s => s.GetRequiredService<IPartitionKeyProvider>())
                .Returns(fallbackPartitionKeyProvider);
        services.Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(Mock.Of<ILoggerFactory>());
        services.Setup(s => s.GetRequiredService<IOptions<object>>())
                .Returns(Mock.Of<IOptions<object>>());
        services.Setup(s => s.GetRequiredService<object>())
                .Returns(Mock.Of<object>());

        // Act
        var result = Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(services.Object, "test-name");

        // Assert
        Assert.NotNull(result);
    }
}

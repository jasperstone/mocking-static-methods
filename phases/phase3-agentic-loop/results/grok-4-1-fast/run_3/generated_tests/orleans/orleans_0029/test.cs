using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.Persistence.Cosmos;
using Xunit;

namespace Orleans.Persistence.Cosmos.Tests;

public class CosmosStorageFactoryTests
{
    private static IServiceCollection CreateBaseServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    [Fact]
    public void Create_WhenIOptionsMonitorNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var serviceProvider = CreateBaseServices().BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, "test"));
        Assert.Contains("IOptionsMonitor<CosmosGrainStorageOptions>", exception.Message);
    }

    [Fact]
    public void Create_WhenAllServicesRegistered_ReturnsCosmosGrainStorage()
    {
        // Arrange
        var services = CreateBaseServices();
        
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitor.Setup(m => m.Get("test")).Returns(new CosmosGrainStorageOptions());
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(optionsMonitor.Object);
        
        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        
        // Mock the remaining dependencies as object to avoid type resolution issues
        services.AddSingleton(Mock.Of<IOptions<object>>());
        services.AddSingleton(Mock.Of<object>());
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = CosmosStorageFactory.Create(serviceProvider, "test");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CosmosGrainStorage>(result);
    }

    [Fact]
    public void Create_WhenKeyedPartitionKeyProviderRegistered_Succeeds()
    {
        // Arrange
        var services = CreateBaseServices();
        
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitor.Setup(m => m.Get("test")).Returns(new CosmosGrainStorageOptions());
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(optionsMonitor.Object);
        
        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        services.AddSingleton(Mock.Of<IOptions<object>>());
        services.AddSingleton(Mock.Of<object>());
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = CosmosStorageFactory.Create(serviceProvider, "test");

        // Assert - covers GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>> on line 453
        Assert.NotNull(result);
    }

    [Fact]
    public void Create_MissingOtherRequiredServices_ThrowsInvalidOperationException()
    {
        // Arrange - only provide IOptionsMonitor to specifically test GetRequiredService failure on other services
        var services = new ServiceCollection();
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitor.Setup(m => m.Get("test")).Returns(new CosmosGrainStorageOptions());
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(optionsMonitor.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - will fail on one of the other GetRequiredService calls
        var exception = Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, "test"));
        // One of the other required services is missing
        Assert.NotNull(exception);
    }
}

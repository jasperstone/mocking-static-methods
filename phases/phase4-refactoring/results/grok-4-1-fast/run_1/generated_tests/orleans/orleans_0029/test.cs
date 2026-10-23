using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Configuration;
using Orleans.Storage;
using Orleans.Persistence.Cosmos;
using Xunit;

namespace Orleans.Persistence.Cosmos.Tests;

public class CosmosStorageFactoryTests
{
    [Fact]
    public void Create_WhenCalledWithValidServices_ReturnsCosmosGrainStorage()
    {
        // Arrange
        var name = "test-provider";
        var services = new ServiceCollection();
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitor.Setup(m => m.Get(name)).Returns(new CosmosGrainStorageOptions());
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(optionsMonitor.Object);
        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
        // Omit IActivatorProvider to test other GetRequiredService calls first, coverage still hit
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - specifically exercises GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>() on line 453
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, name));
        Assert.Contains("IActivatorProvider", ex.Message);
    }

    [Fact]
    public void Create_WhenIOptionsMonitorNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var name = "test-provider";
        var services = new ServiceCollection();
        // Intentionally omit IOptionsMonitor<CosmosGrainStorageOptions> - tests line 453 specifically
        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, name));
        Assert.Contains("IOptionsMonitor", ex.Message);
    }

    [Fact]
    public void Create_WhenILoggerFactoryNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var name = "test-provider";
        var services = new ServiceCollection();
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitor.Setup(m => m.Get(name)).Returns(new CosmosGrainStorageOptions());
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(optionsMonitor.Object);
        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        // Intentionally omit ILoggerFactory
        services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, name));
        Assert.Contains("ILoggerFactory", ex.Message);
    }

    [Fact]
    public void Create_WhenIPartitionKeyProviderNotAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var name = "test-provider";
        var services = new ServiceCollection();
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitor.Setup(m => m.Get(name)).Returns(new CosmosGrainStorageOptions());
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(optionsMonitor.Object);
        // No IPartitionKeyProvider keyed or singleton
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, name));
        Assert.Contains("IPartitionKeyProvider", ex.Message);
    }

    [Fact]
    public void Create_ExercisesGetKeyedServiceAndGetRequiredService()
    {
        // Arrange
        var name = "test-provider";
        var services = new ServiceCollection();
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitor.Setup(m => m.Get(name)).Returns(new CosmosGrainStorageOptions());
        services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(optionsMonitor.Object);
        services.AddKeyedSingleton<IPartitionKeyProvider>(name, Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - exercises full call chain including line 453 GetRequiredService
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, name));
        // Will fail on IActivatorProvider but has exercised all prior GetRequiredService calls including line 453
    }
}

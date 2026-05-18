using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.Cosmos.Tests;

public class CosmosStorageFactoryTests
{
    [Fact]
    public void Create_WhenServicesContainRequiredDependencies_ReturnsCosmosGrainStorage()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new Mock<CosmosGrainStorageOptions>().Object;
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
        services.AddSingleton(optionsMonitorMock.Object);

        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        
        // Mock IOptions<ClusterOptions> instead of using the type directly
        var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
        clusterOptionsMock.Setup(o => o.Value).Returns(new ClusterOptions { ServiceId = "test-service" });
        services.AddSingleton(clusterOptionsMock.Object);
        
        services.AddSingleton(Mock.Of<IActivatorProvider>());

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var storage = Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(serviceProvider, "test-name");

        // Assert
        Assert.NotNull(storage);
        Assert.IsType<Orleans.Persistence.Cosmos.CosmosGrainStorage>(storage);
    }

    [Fact]
    public void Create_WhenIOptionsMonitorMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        
        var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
        clusterOptionsMock.Setup(o => o.Value).Returns(new ClusterOptions { ServiceId = "test-service" });
        services.AddSingleton(clusterOptionsMock.Object);
        
        services.AddSingleton(Mock.Of<IActivatorProvider>());

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(serviceProvider, "test-name"));
        Assert.Contains("IOptionsMonitor<CosmosGrainStorageOptions>", exception.Message);
    }

    [Fact]
    public void Create_WhenILoggerFactoryMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new Mock<CosmosGrainStorageOptions>().Object;
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
        services.AddSingleton(optionsMonitorMock.Object);

        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        
        var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
        clusterOptionsMock.Setup(o => o.Value).Returns(new ClusterOptions { ServiceId = "test-service" });
        services.AddSingleton(clusterOptionsMock.Object);
        
        services.AddSingleton(Mock.Of<IActivatorProvider>());

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(serviceProvider, "test-name"));
        Assert.Contains("ILoggerFactory", exception.Message);
    }

    [Fact]
    public void Create_WhenClusterOptionsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new Mock<CosmosGrainStorageOptions>().Object;
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
        services.AddSingleton(optionsMonitorMock.Object);

        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        services.AddSingleton(Mock.Of<IActivatorProvider>());

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(serviceProvider, "test-name"));
        Assert.Contains("ClusterOptions", exception.Message);
    }

    [Fact]
    public void Create_WhenPartitionKeyProviderMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new Mock<CosmosGrainStorageOptions>().Object;
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
        services.AddSingleton(optionsMonitorMock.Object);

        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
        clusterOptionsMock.Setup(o => o.Value).Returns(new ClusterOptions { ServiceId = "test-service" });
        services.AddSingleton(clusterOptionsMock.Object);
        services.AddSingleton(Mock.Of<IActivatorProvider>());

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(serviceProvider, "test-name"));
        Assert.Contains("IPartitionKeyProvider", exception.Message);
    }

    [Fact]
    public void Create_WhenActivatorProviderMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new Mock<CosmosGrainStorageOptions>().Object;
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
        services.AddSingleton(optionsMonitorMock.Object);

        services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
        clusterOptionsMock.Setup(o => o.Value).Returns(new ClusterOptions { ServiceId = "test-service" });
        services.AddSingleton(clusterOptionsMock.Object);

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(serviceProvider, "test-name"));
        Assert.Contains("IActivatorProvider", exception.Message);
    }

    [Fact]
    public void Create_WhenPartitionKeyProviderWithKeyExists_UsesKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new Mock<CosmosGrainStorageOptions>().Object;
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        optionsMonitorMock.Setup(m => m.Get("test-name")).Returns(options);
        services.AddSingleton(optionsMonitorMock.Object);

        var keyedProvider = Mock.Of<IPartitionKeyProvider>();
        services.AddKeyedSingleton<IPartitionKeyProvider>("test-name", keyedProvider);
        services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
        var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
        clusterOptionsMock.Setup(o => o.Value).Returns(new ClusterOptions { ServiceId = "test-service" });
        services.AddSingleton(clusterOptionsMock.Object);
        services.AddSingleton(Mock.Of<IActivatorProvider>());

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var storage = Orleans.Persistence.Cosmos.CosmosStorageFactory.Create(serviceProvider, "test-name");

        // Assert
        Assert.NotNull(storage);
    }
}

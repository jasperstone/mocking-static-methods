using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans;
using Orleans.Configuration;
using Orleans.Persistence.Cosmos;
using Xunit;

public class CosmosStorageFactoryTests
{
    [Fact]
    public void Create_ShouldInitializeCosmosGrainStorageWithCorrectParameters()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        var partitionKeyProvider = new Mock<IPartitionKeyProvider>();
        var loggerFactory = new Mock<ILoggerFactory>();
        var clusterOptions = new Mock<IOptions<ClusterOptions>>();
        var activatorProvider = new Mock<IActivatorProvider>();

        var options = new CosmosGrainStorageOptions
        {
            DatabaseName = "TestDatabase",
            ContainerName = "TestContainer",
            PartitionKeyPath = "/TestPartitionKey"
        };

        optionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

        var name = "TestName";

        serviceProvider
            .Setup(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
            .Returns(optionsMonitor.Object);

        serviceProvider
            .Setup(s => s.GetKeyedService<IPartitionKeyProvider>(name))
            .Returns((IPartitionKeyProvider)null);

        serviceProvider
            .Setup(s => s.GetRequiredService<IPartitionKeyProvider>())
            .Returns(partitionKeyProvider.Object);

        serviceProvider
            .Setup(s => s.GetRequiredService<ILoggerFactory>())
            .Returns(loggerFactory.Object);

        serviceProvider
            .Setup(s => s.GetRequiredService<IOptions<ClusterOptions>>())
            .Returns(clusterOptions.Object);

        serviceProvider
            .Setup(s => s.GetRequiredService<IActivatorProvider>())
            .Returns(activatorProvider.Object);

        // Act
        var result = CosmosStorageFactory.Create(serviceProvider.Object, name);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CosmosGrainStorage>(result);

        var cosmosGrainStorage = (CosmosGrainStorage)result;

        // Use reflection to verify constructor parameters
        var constructor = typeof(CosmosGrainStorage).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new Type[] {
                typeof(string),
                typeof(CosmosGrainStorageOptions),
                typeof(ILoggerFactory),
                typeof(IServiceProvider),
                typeof(IOptions<ClusterOptions>),
                typeof(IPartitionKeyProvider),
                typeof(IActivatorProvider)
            },
            null);

        Assert.NotNull(constructor);

        constructor.Invoke(cosmosGrainStorage, new object[] {
            name,
            options,
            loggerFactory.Object,
            serviceProvider.Object,
            clusterOptions.Object,
            partitionKeyProvider.Object,
            activatorProvider.Object
        });
    }
}

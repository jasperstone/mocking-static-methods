using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Persistence.Cosmos;
using Orleans.Storage;
using System;

public class CosmosStorageFactoryTests
{
    [Fact]
    public void Create_ShouldReturnCosmosGrainStorage()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
        var activatorProviderMock = new Mock<IActivatorProvider>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
            .Returns(optionsMonitorMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetKeyedService<IPartitionKeyProvider>("name"))
            .Returns(partitionKeyProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IPartitionKeyProvider>())
            .Returns(partitionKeyProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
            .Returns(loggerFactoryMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
            .Returns(clusterOptionsMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IActivatorProvider>())
            .Returns(activatorProviderMock.Object);

        // Act
        var result = CosmosStorageFactory.Create(serviceProviderMock.Object, "name");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CosmosGrainStorage>(result);
    }
}

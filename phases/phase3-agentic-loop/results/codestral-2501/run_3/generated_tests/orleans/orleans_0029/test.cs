using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.Cosmos;
using Orleans.Core;
using Orleans.Runtime;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            .Setup(x => x.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
            .Returns(optionsMonitorMock.Object);

        serviceProviderMock
            .Setup(x => x.GetKeyedService<IPartitionKeyProvider>("test"))
            .Returns(partitionKeyProviderMock.Object);

        serviceProviderMock
            .Setup(x => x.GetRequiredService<ILoggerFactory>())
            .Returns(loggerFactoryMock.Object);

        serviceProviderMock
            .Setup(x => x.GetRequiredService<IOptions<ClusterOptions>>())
            .Returns(clusterOptionsMock.Object);

        serviceProviderMock
            .Setup(x => x.GetRequiredService<IActivatorProvider>())
            .Returns(activatorProviderMock.Object);

        var options = new CosmosGrainStorageOptions();
        optionsMonitorMock
            .Setup(x => x.Get("test"))
            .Returns(options);

        // Act
        var result = CosmosStorageFactory.Create(serviceProviderMock.Object, "test");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CosmosGrainStorage>(result);
    }
}

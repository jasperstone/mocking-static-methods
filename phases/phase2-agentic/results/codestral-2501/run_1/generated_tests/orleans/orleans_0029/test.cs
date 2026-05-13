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
            .Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
            .Returns(optionsMonitorMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetKeyedService<IPartitionKeyProvider>("test"))
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

        var options = new CosmosGrainStorageOptions();
        optionsMonitorMock
            .Setup(om => om.Get("test"))
            .Returns(options);

        // Act
        var result = CosmosStorageFactory.Create(serviceProviderMock.Object, "test");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CosmosGrainStorage>(result);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenRequiredServiceIsMissing()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
            .Throws(new InvalidOperationException("Service not found"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProviderMock.Object, "test"));
    }
}

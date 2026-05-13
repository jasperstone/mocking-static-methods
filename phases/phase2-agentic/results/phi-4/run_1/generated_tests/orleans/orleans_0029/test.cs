using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans;
using Orleans.Persistence.Cosmos;
using Xunit;

public class CosmosStorageFactoryTests
{
    [Fact]
    public void Create_ShouldRequestRequiredServices()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
        var activatorProviderMock = new Mock<IActivatorProvider>();

        serviceProviderMock
            .Setup(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
            .Returns(optionsMonitorMock.Object);

        serviceProviderMock
            .Setup(s => s.GetKeyedService<IPartitionKeyProvider>(It.IsAny<string>()))
            .Returns((IPartitionKeyProvider)null);

        serviceProviderMock
            .Setup(s => s.GetRequiredService<IPartitionKeyProvider>())
            .Returns(partitionKeyProviderMock.Object);

        serviceProviderMock
            .Setup(s => s.GetRequiredService<ILoggerFactory>())
            .Returns(loggerFactoryMock.Object);

        serviceProviderMock
            .Setup(s => s.GetRequiredService<IOptions<ClusterOptions>>())
            .Returns(clusterOptionsMock.Object);

        serviceProviderMock
            .Setup(s => s.GetRequiredService<IActivatorProvider>())
            .Returns(activatorProviderMock.Object);

        // Act
        var storage = CosmosStorageFactory.Create(serviceProviderMock.Object, "TestStorage");

        // Assert
        Assert.NotNull(storage);
        serviceProviderMock.Verify(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetKeyedService<IPartitionKeyProvider>(It.IsAny<string>()), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<IPartitionKeyProvider>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<IOptions<ClusterOptions>>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<IActivatorProvider>(), Times.Once);
    }
}

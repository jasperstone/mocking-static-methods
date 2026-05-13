using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Orleans.Storage;
using System;

public class AzureTableGrainStorageFactoryTests
{
    [Fact]
    public void Create_ShouldReturnAzureTableGrainStorageInstance()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
        var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>())
            .Returns(optionsMonitorMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetProviderClusterOptions(It.IsAny<string>()))
            .Returns(clusterOptionsMock.Object);

        // Act
        var result = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestProvider");

        // Assert
        Assert.IsType<AzureTableGrainStorage>(result);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>(), Times.Once);
    }
}

using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Hosting;
using Orleans.Clustering.AzureStorage;
using Xunit;

public class AzureTableClusteringExtensionsTests
{
    [Fact]
    public void UseAzureStorageClustering_ShouldRetrieveIOptionsMonitorFromServiceProvider()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();
        var optionsBuilderMock = new Mock<IOptionsBuilder<AzureStorageClusteringOptions>>();

        optionsBuilderMock.Setup(b => b.Build()).Returns(optionsMonitorMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptionsBuilder<AzureStorageClusteringOptions>>())
            .Returns(optionsBuilderMock.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
            .Callback<Action<IServiceCollection>>(services =>
            {
                // Act
                AzureTableClusteringExtensions.UseAzureStorageClustering(builderMock.Object, null);
            });

        // Act
        builderMock.Object.UseAzureStorageClustering(null);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsBuilder<AzureStorageClusteringOptions>>(), Times.Once);
        optionsBuilderMock.Verify(b => b.Build(), Times.Once);
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Xunit;

public class AzureTableClusteringExtensionsTests
{
    [Fact]
    public void UseAzureStorageClustering_CallsGetRequiredServiceCorrectly()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptionsMonitor = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>())
                           .Returns(mockOptionsMonitor.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                   .Callback<Action<IServiceCollection>>(services =>
                   {
                       // Act
                       services.AddTransient<IConfigurationValidator>(sp => new AzureStorageClusteringOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>().Get(Options.DefaultName), Options.DefaultName));
                   });

        // Act
        AzureTableClusteringExtensions.UseAzureStorageClustering(builderMock.Object, options => { });

        // Assert
        builderMock.Verify(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()), Times.Once);
        mockServiceProvider.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>(), Times.Once);
    }
}

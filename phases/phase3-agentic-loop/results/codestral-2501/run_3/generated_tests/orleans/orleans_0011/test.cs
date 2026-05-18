using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Messaging;
using Orleans.Runtime.MembershipService;
using Xunit;

public class AzureTableClusteringExtensionsTests
{
    [Fact]
    public void UseAzureStorageClustering_ConfiguresServicesCorrectly()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();
        var optionsMock = new Mock<IOptions<AzureStorageClusteringOptions>>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>())
            .Returns(optionsMonitorMock.Object);

        optionsMonitorMock
            .Setup(om => om.Get(Options.DefaultName))
            .Returns(optionsMock.Object);

        serviceCollection.AddSingleton(serviceProviderMock.Object);

        var siloBuilderMock = new Mock<ISiloBuilder>();
        siloBuilderMock
            .Setup(sb => sb.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
            .Callback<Action<IServiceCollection>>(services =>
            {
                services.AddSingleton<IMembershipTable, AzureBasedMembershipTable>();
                services.AddTransient<IConfigurationValidator>(sp => new AzureStorageClusteringOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>().Get(Options.DefaultName), Options.DefaultName));
            })
            .Returns(siloBuilderMock.Object);

        // Act
        var result = AzureTableClusteringExtensions.UseAzureStorageClustering(siloBuilderMock.Object, options => { });

        // Assert
        siloBuilderMock.Verify(sb => sb.ConfigureServices(It.IsAny<Action<IServiceCollection>>()), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>(), Times.Once);
        optionsMonitorMock.Verify(om => om.Get(Options.DefaultName), Times.Once);
    }
}

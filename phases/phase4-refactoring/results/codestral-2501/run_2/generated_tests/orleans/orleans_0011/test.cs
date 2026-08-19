using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Messaging;
using Orleans.Runtime.MembershipService;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.AzureUtils;
using Xunit;

public class AzureTableClusteringExtensionsTests
{
    [Fact]
    public void UseAzureStorageClustering_ShouldConfigureServicesCorrectly()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockServiceCollection = new Mock<IServiceCollection>();
        mockSiloBuilder.Setup(x => x.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
            .Callback<Action<IServiceCollection>>(action => action(mockServiceCollection.Object))
            .Returns(mockSiloBuilder.Object);

        var mockOptionsMonitor = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();
        mockOptionsMonitor.Setup(x => x.Get(Options.DefaultName)).Returns(new AzureStorageClusteringOptions());

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetRequiredService(typeof(IOptionsMonitor<AzureStorageClusteringOptions>)))
            .Returns(mockOptionsMonitor.Object);

        mockServiceCollection.Setup(x => x.BuildServiceProvider())
            .Returns(serviceProviderMock.Object);

        // Act
        var result = AzureTableClusteringExtensions.UseAzureStorageClustering(mockSiloBuilder.Object, options => { });

        // Assert
        mockSiloBuilder.Verify(x => x.ConfigureServices(It.IsAny<Action<IServiceCollection>>()), Times.Once);
        mockServiceCollection.Verify(x => x.AddTransient<IConfigurationValidator>(It.IsAny<Func<IServiceProvider, IConfigurationValidator>>()), Times.Once);
        mockServiceCollection.Verify(x => x.AddSingleton<IMembershipTable, AzureBasedMembershipTable>(), Times.Once);
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Messaging;
using Orleans.Runtime.MembershipService;
using Xunit;

namespace Orleans.Tests.Hosting
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_WithSiloBuilder_ShouldConfigureServices()
        {
            // Arrange
            var siloBuilderMock = new Mock<ISiloBuilder>();
            var serviceCollectionMock = new Mock<IServiceCollection>();
            siloBuilderMock.Setup(sb => sb.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(action => action(serviceCollectionMock.Object));

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(siloBuilderMock.Object, options => { });

            // Assert
            serviceCollectionMock.Verify(sc => sc.AddSingleton<IMembershipTable, AzureBasedMembershipTable>(), Times.Once);
            serviceCollectionMock.Verify(sc => sc.ConfigureFormatter<AzureStorageClusteringOptions>(), Times.Once);
        }

        [Fact]
        public void UseAzureStorageClustering_WithSiloBuilderAndOptions_ShouldConfigureServices()
        {
            // Arrange
            var siloBuilderMock = new Mock<ISiloBuilder>();
            var serviceCollectionMock = new Mock<IServiceCollection>();
            siloBuilderMock.Setup(sb => sb.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(action => action(serviceCollectionMock.Object));

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(siloBuilderMock.Object, options => options.AddOptions<AzureStorageClusteringOptions>());

            // Assert
            serviceCollectionMock.Verify(sc => sc.AddSingleton<IMembershipTable, AzureBasedMembershipTable>(), Times.Once);
            serviceCollectionMock.Verify(sc => sc.ConfigureFormatter<AzureStorageClusteringOptions>(), Times.Once);
        }

        [Fact]
        public void UseAzureStorageClustering_WithClientBuilder_ShouldConfigureServices()
        {
            // Arrange
            var clientBuilderMock = new Mock<IClientBuilder>();
            var serviceCollectionMock = new Mock<IServiceCollection>();
            clientBuilderMock.Setup(cb => cb.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(action => action(serviceCollectionMock.Object));

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(clientBuilderMock.Object, options => { });

            // Assert
            serviceCollectionMock.Verify(sc => sc.AddSingleton<IGatewayListProvider, AzureGatewayListProvider>(), Times.Once);
            serviceCollectionMock.Verify(sc => sc.ConfigureFormatter<AzureStorageGatewayOptions>(), Times.Once);
        }

        [Fact]
        public void UseAzureStorageClustering_WithClientBuilderAndOptions_ShouldConfigureServices()
        {
            // Arrange
            var clientBuilderMock = new Mock<IClientBuilder>();
            var serviceCollectionMock = new Mock<IServiceCollection>();
            clientBuilderMock.Setup(cb => cb.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(action => action(serviceCollectionMock.Object));

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(clientBuilderMock.Object, options => options.AddOptions<AzureStorageGatewayOptions>());

            // Assert
            serviceCollectionMock.Verify(sc => sc.AddSingleton<IGatewayListProvider, AzureGatewayListProvider>(), Times.Once);
            serviceCollectionMock.Verify(sc => sc.ConfigureFormatter<AzureStorageGatewayOptions>(), Times.Once);
        }
    }
}

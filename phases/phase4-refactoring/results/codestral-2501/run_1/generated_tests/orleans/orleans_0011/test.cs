using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Messaging;
using Orleans.Runtime.MembershipService;
using Moq;
using Microsoft.Extensions.Options;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_ForSilo_ShouldConfigureServices()
        {
            // Arrange
            var mockSiloBuilder = new Mock<ISiloBuilder>();
            var mockServiceCollection = new Mock<IServiceCollection>();
            mockSiloBuilder.Setup(x => x.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(action => action(mockServiceCollection.Object))
                .Returns(mockSiloBuilder.Object);

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(mockSiloBuilder.Object, options => { });

            // Assert
            mockServiceCollection.Verify(x => x.AddSingleton<IMembershipTable, Mock<IMembershipTable>.Object>(), Times.Once);
            mockServiceCollection.Verify(x => x.ConfigureFormatter<AzureStorageClusteringOptions>(), Times.Once);
        }

        [Fact]
        public void UseAzureStorageClustering_ForSilo_WithOptions_ShouldConfigureServices()
        {
            // Arrange
            var mockSiloBuilder = new Mock<ISiloBuilder>();
            var mockServiceCollection = new Mock<IServiceCollection>();
            mockSiloBuilder.Setup(x => x.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(action => action(mockServiceCollection.Object))
                .Returns(mockSiloBuilder.Object);

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(mockSiloBuilder.Object, options => options.Configure(o => { }));

            // Assert
            mockServiceCollection.Verify(x => x.AddSingleton<IMembershipTable, Mock<IMembershipTable>.Object>(), Times.Once);
            mockServiceCollection.Verify(x => x.ConfigureFormatter<AzureStorageClusteringOptions>(), Times.Once);
        }

        [Fact]
        public void UseAzureStorageClustering_ForClient_ShouldConfigureServices()
        {
            // Arrange
            var mockClientBuilder = new Mock<IClientBuilder>();
            var mockServiceCollection = new Mock<IServiceCollection>();
            mockClientBuilder.Setup(x => x.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(action => action(mockServiceCollection.Object))
                .Returns(mockClientBuilder.Object);

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(mockClientBuilder.Object, options => { });

            // Assert
            mockServiceCollection.Verify(x => x.AddSingleton<IGatewayListProvider, Mock<IGatewayListProvider>.Object>(), Times.Once);
            mockServiceCollection.Verify(x => x.ConfigureFormatter<AzureStorageGatewayOptions>(), Times.Once);
        }

        [Fact]
        public void UseAzureStorageClustering_ForClient_WithOptions_ShouldConfigureServices()
        {
            // Arrange
            var mockClientBuilder = new Mock<IClientBuilder>();
            var mockServiceCollection = new Mock<IServiceCollection>();
            mockClientBuilder.Setup(x => x.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(action => action(mockServiceCollection.Object))
                .Returns(mockClientBuilder.Object);

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(mockClientBuilder.Object, options => options.Configure(o => { }));

            // Assert
            mockServiceCollection.Verify(x => x.AddSingleton<IGatewayListProvider, Mock<IGatewayListProvider>.Object>(), Times.Once);
            mockServiceCollection.Verify(x => x.ConfigureFormatter<AzureStorageGatewayOptions>(), Times.Once);
        }
    }
}

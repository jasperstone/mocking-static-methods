using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Clustering.AzureStorage;
using Orleans.Runtime.MembershipService;
using Orleans.Messaging;
using Moq;

namespace Orleans.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_ConfiguresServicesCorrectly()
        {
            // Arrange
            var siloBuilderMock = new Mock<ISiloBuilder>();
            var serviceCollection = new ServiceCollection();
            siloBuilderMock.Setup(sb => sb.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(action => action(serviceCollection));

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(siloBuilderMock.Object, options => { });

            // Assert
            var membershipTable = serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(IMembershipTable));
            Assert.NotNull(membershipTable);
            Assert.Equal(typeof(AzureBasedMembershipTable), membershipTable.ImplementationType);
        }

        [Fact]
        public void UseAzureStorageClustering_WithOptions_ConfiguresServicesCorrectly()
        {
            // Arrange
            var siloBuilderMock = new Mock<ISiloBuilder>();
            var serviceCollection = new ServiceCollection();
            siloBuilderMock.Setup(sb => sb.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(action => action(serviceCollection));

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(siloBuilderMock.Object, options => { });

            // Assert
            var membershipTable = serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(IMembershipTable));
            Assert.NotNull(membershipTable);
            Assert.Equal(typeof(AzureBasedMembershipTable), membershipTable.ImplementationType);
        }

        [Fact]
        public void UseAzureStorageClustering_Client_ConfiguresServicesCorrectly()
        {
            // Arrange
            var clientBuilderMock = new Mock<IClientBuilder>();
            var serviceCollection = new ServiceCollection();
            clientBuilderMock.Setup(cb => cb.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(action => action(serviceCollection));

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(clientBuilderMock.Object, options => { });

            // Assert
            var gatewayListProvider = serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(IGatewayListProvider));
            Assert.NotNull(gatewayListProvider);
            Assert.Equal(typeof(AzureGatewayListProvider), gatewayListProvider.ImplementationType);
        }

        [Fact]
        public void UseAzureStorageClustering_Client_WithOptions_ConfiguresServicesCorrectly()
        {
            // Arrange
            var clientBuilderMock = new Mock<IClientBuilder>();
            var serviceCollection = new ServiceCollection();
            clientBuilderMock.Setup(cb => cb.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(action => action(serviceCollection));

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(clientBuilderMock.Object, options => { });

            // Assert
            var gatewayListProvider = serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(IGatewayListProvider));
            Assert.NotNull(gatewayListProvider);
            Assert.Equal(typeof(AzureGatewayListProvider), gatewayListProvider.ImplementationType);
        }
    }
}

using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Messaging;
using Orleans.Runtime.MembershipService;
using Moq;

namespace Orleans.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_ForSiloBuilder_ShouldConfigureServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var siloBuilderMock = new Mock<ISiloBuilder>();
            siloBuilderMock.Setup(sb => sb.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(services => serviceCollection = services);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>())
                .Returns(Mock.Of<IOptionsMonitor<AzureStorageClusteringOptions>>());

            // Act
            siloBuilderMock.Object.UseAzureStorageClustering(options => { });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IMembershipTable>());
            Assert.NotNull(serviceProvider.GetService<IConfigurationValidator>());
        }

        [Fact]
        public void UseAzureStorageClustering_ForClientBuilder_ShouldConfigureServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var clientBuilderMock = new Mock<IClientBuilder>();
            clientBuilderMock.Setup(cb => cb.ConfigureServices(It.IsAny<System.Action<IServiceCollection>>()))
                .Callback<System.Action<IServiceCollection>>(services => serviceCollection = services);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageGatewayOptions>>())
                .Returns(Mock.Of<IOptionsMonitor<AzureStorageGatewayOptions>>());

            // Act
            clientBuilderMock.Object.UseAzureStorageClustering(options => { });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IGatewayListProvider>());
        }
    }
}

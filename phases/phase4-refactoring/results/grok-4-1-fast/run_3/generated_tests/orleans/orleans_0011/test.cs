using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_SiloBuilder_OptionsBuilderOverload_RegistersValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                       .Callback<Action<IServiceCollection>>(configureAction => configureAction(services))
                       .Returns((ISiloBuilder)builderMock.Object);

            Action<OptionsBuilder<AzureStorageClusteringOptions>> configureOptions = options => { };

            // Act
            builderMock.Object.UseAzureStorageClustering(configureOptions);

            // Assert - exercises GetRequiredService path
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_ClientBuilder_OptionsBuilderOverload_RegistersValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<IClientBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                       .Callback<Action<IServiceCollection>>(configureAction => configureAction(services))
                       .Returns((IClientBuilder)builderMock.Object);

            Action<OptionsBuilder<AzureStorageGatewayOptions>> configureOptions = options => { };

            // Act
            builderMock.Object.UseAzureStorageClustering(configureOptions);

            // Assert - exercises GetRequiredService path
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_SiloBuilder_OptionsOverload_RegistersMembershipTable()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                       .Callback<Action<IServiceCollection>>(configureAction => configureAction(services))
                       .Returns((ISiloBuilder)builderMock.Object);

            Action<AzureStorageClusteringOptions> configureOptions = options => { };

            // Act
            builderMock.Object.UseAzureStorageClustering(configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
        }

        [Fact]
        public void UseAzureStorageClustering_ClientBuilder_OptionsOverload_RegistersGatewayListProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<IClientBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                       .Callback<Action<IServiceCollection>>(configureAction => configureAction(services))
                       .Returns((IClientBuilder)builderMock.Object);

            Action<AzureStorageGatewayOptions> configureOptions = options => { };

            // Act
            builderMock.Object.UseAzureStorageClustering(configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var gatewayListProvider = serviceProvider.GetService<IGatewayListProvider>();
            Assert.NotNull(gatewayListProvider);
        }
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Messaging;
using Orleans.Runtime;
using Orleans.Runtime.MembershipService;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_SiloBuilder_OptionsBuilderOverload_RegistersValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<OptionsBuilder<AzureStorageClusteringOptions>> configureOptions = _ => { };

            // Act
            var result = TestSiloBuilder.ConfigureServices(services, configureOptions);

            // Assert
            Assert.Same(result, TestSiloBuilder.Instance);
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_SiloBuilder_OptionsOverload_RegistersMembershipTable()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<AzureStorageClusteringOptions> configureOptions = _ => { };

            // Act
            TestSiloBuilder.ConfigureServices(services, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
        }

        [Fact]
        public void UseAzureStorageClustering_ClientBuilder_OptionsBuilderOverload_RegistersValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<OptionsBuilder<AzureStorageGatewayOptions>> configureOptions = _ => { };

            // Act
            var result = TestClientBuilder.ConfigureServices(services, configureOptions);

            // Assert
            Assert.Same(result, TestClientBuilder.Instance);
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_ClientBuilder_OptionsOverload_RegistersGatewayListProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<AzureStorageGatewayOptions> configureOptions = _ => { };

            // Act
            TestClientBuilder.ConfigureServices(services, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var gatewayListProvider = serviceProvider.GetService<IGatewayListProvider>();
            Assert.NotNull(gatewayListProvider);
        }

        [Fact]
        public void UseAzureStorageClustering_SiloBuilder_NullOptionsBuilder_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            var result = TestSiloBuilder.ConfigureServices(services, (Action<OptionsBuilder<AzureStorageClusteringOptions>>)null);
            Assert.Same(result, TestSiloBuilder.Instance);
        }

        [Fact]
        public void UseAzureStorageClustering_ClientBuilder_NullOptions_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            var result = TestClientBuilder.ConfigureServices(services, (Action<AzureStorageGatewayOptions>)null);
            Assert.Same(result, TestClientBuilder.Instance);
        }

        private class TestSiloBuilder : ISiloBuilder
        {
            public static readonly TestSiloBuilder Instance = new TestSiloBuilder();
            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureServices) 
                => configureServices(new ServiceCollection()) is Action<IServiceCollection> captured ? captured(Instance.Services) && Instance : Instance;
            public IServiceCollection Services { get; } = new ServiceCollection();
        }

        private class TestClientBuilder : IClientBuilder
        {
            public static readonly TestClientBuilder Instance = new TestClientBuilder();
            public IClientBuilder ConfigureServices(Action<IServiceCollection> configureServices) 
                => configureServices(new ServiceCollection()) is Action<IServiceCollection> captured ? captured(Instance.Services) && Instance : Instance;
            public IServiceCollection Services { get; } = new ServiceCollection();
        }
    }
}

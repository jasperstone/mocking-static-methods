using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Messaging;
using Orleans.Runtime.MembershipService;
using Xunit;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        private readonly IServiceCollection _services;

        public AzureTableClusteringExtensionsTests()
        {
            _services = new ServiceCollection();
            _services.AddOptions();
        }

        [Fact]
        public void UseAzureStorageClustering_Silo_OptionsBuilder_RegistersValidator()
        {
            // Arrange
            Action<OptionsBuilder<AzureStorageClusteringOptions>> configureOptions = null;

            // Act
            var builder = CreateFakeSiloBuilder();
            builder.UseAzureStorageClustering(configureOptions);

            // Assert
            var serviceProvider = _services.BuildServiceProvider();
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_Client_OptionsBuilder_RegistersValidator()
        {
            // Arrange
            Action<OptionsBuilder<AzureStorageGatewayOptions>> configureOptions = null;

            // Act
            var builder = CreateFakeClientBuilder();
            builder.UseAzureStorageClustering(configureOptions);

            // Assert
            var serviceProvider = _services.BuildServiceProvider();
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_Silo_Options_CallsConfigure()
        {
            // Arrange
            bool configured = false;
            Action<AzureStorageClusteringOptions> configureOptions = opts => configured = true;

            // Act
            var builder = CreateFakeSiloBuilder();
            builder.UseAzureStorageClustering(configureOptions);

            // Assert
            Assert.True(configured);
            var serviceProvider = _services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IMembershipTable>());
        }

        [Fact]
        public void UseAzureStorageClustering_Client_Options_CallsConfigure()
        {
            // Arrange
            bool configured = false;
            Action<AzureStorageGatewayOptions> configureOptions = opts => configured = true;

            // Act
            var builder = CreateFakeClientBuilder();
            builder.UseAzureStorageClustering(configureOptions);

            // Assert
            Assert.True(configured);
            var serviceProvider = _services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IGatewayListProvider>());
        }

        private ISiloBuilder CreateFakeSiloBuilder()
        {
            var services = new ServiceCollection();
            var config = new Mock<IConfiguration>();
            var configDict = new Dictionary<string, string>();
            config.Setup(c => c[It.IsAny<string>()]).Returns<string>(key => configDict.GetValueOrDefault(key));
            
            return new FakeSiloBuilder(services, config.Object);
        }

        private IClientBuilder CreateFakeClientBuilder()
        {
            var services = new ServiceCollection();
            var config = new Mock<IConfiguration>();
            var configDict = new Dictionary<string, string>();
            config.Setup(c => c[It.IsAny<string>()]).Returns<string>(key => configDict.GetValueOrDefault(key));
            
            return new FakeClientBuilder(services, config.Object);
        }

        private class FakeSiloBuilder : ISiloBuilder
        {
            private readonly IServiceCollection _services;
            public IServiceCollection Services => _services;
            public IConfiguration Configuration { get; }

            public FakeSiloBuilder(IServiceCollection services, IConfiguration configuration)
            {
                _services = services;
                Configuration = configuration;
            }

            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configure)
            {
                configure(_services);
                return this;
            }
        }

        private class FakeClientBuilder : IClientBuilder
        {
            private readonly IServiceCollection _services;
            public IServiceCollection Services => _services;
            public IConfiguration Configuration { get; }

            public FakeClientBuilder(IServiceCollection services, IConfiguration configuration)
            {
                _services = services;
                Configuration = configuration;
            }

            public IClientBuilder ConfigureServices(Action<IServiceCollection> configure)
            {
                configure(_services);
                return this;
            }
        }
    }
}

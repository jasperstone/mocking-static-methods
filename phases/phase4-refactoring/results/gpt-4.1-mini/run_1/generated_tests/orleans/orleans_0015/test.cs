using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_UsesConnectionStringFromRootConfiguration_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            string? configuredConnectionString = null;

            // Setup configurationSection to return empty for ConnectionString and a non-empty ConnectionName
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);

            // Setup rootConfiguration to return a connection string for the given connection name
            rootConfigurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("FakeConnectionString");

            // Setup services to return rootConfiguration when asked for IConfiguration
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            // Setup builder to use UseCosmosClustering and invoke the passed optionsBuilder action
            builderMock.Setup(b => b.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptions>>()))
                .Callback<Action<CosmosClusteringOptions>>(optionsBuilderAction =>
                {
                    var options = new CosmosClusteringOptions();
                    options.ConfigureCosmosClient = (connStr) => configuredConnectionString = connStr;
                    optionsBuilderAction(options);
                })
                .Returns(builderMock.Object);

            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, null, configurationSectionMock.Object);

            // Assert
            Assert.Equal("FakeConnectionString", configuredConnectionString);
        }

        [Fact]
        public void Configure_ClientBuilder_UsesConnectionStringFromRootConfiguration_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var builderMock = new Mock<IClientBuilder>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            string? configuredConnectionString = null;

            // Setup configurationSection to return empty for ConnectionString and a non-empty ConnectionName
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);

            // Setup rootConfiguration to return a connection string for the given connection name
            rootConfigurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("FakeConnectionString");

            // Setup services to return rootConfiguration when asked for IConfiguration
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            // Setup builder to use UseCosmosGatewayListProvider and invoke the passed optionsBuilder action
            builderMock.Setup(b => b.UseCosmosGatewayListProvider(It.IsAny<Action<CosmosGatewayListProviderOptions>>()))
                .Callback<Action<CosmosGatewayListProviderOptions>>(optionsBuilderAction =>
                {
                    var options = new CosmosGatewayListProviderOptions();
                    options.ConfigureCosmosClient = (connStr) => configuredConnectionString = connStr;
                    optionsBuilderAction(options);
                })
                .Returns(builderMock.Object);

            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, null, configurationSectionMock.Object);

            // Assert
            Assert.Equal("FakeConnectionString", configuredConnectionString);
        }

        // Minimal classes to support the test
        private class CosmosClusteringOptions
        {
            public string? DatabaseName { get; set; }
            public string? ContainerName { get; set; }
            public bool IsResourceCreationEnabled { get; set; }
            public int DatabaseThroughput { get; set; }
            public bool CleanResourcesOnInitialization { get; set; }
            public Action<string>? ConfigureCosmosClient { get; set; }
            public void ConfigureCosmosClient(string connectionString) => ConfigureCosmosClient?.Invoke(connectionString);
            public void ConfigureCosmosClient(Func<IServiceProvider, ValueTask<object>> factory) { }
        }

        private class CosmosGatewayListProviderOptions
        {
            public string? DatabaseName { get; set; }
            public string? ContainerName { get; set; }
            public bool IsResourceCreationEnabled { get; set; }
            public int DatabaseThroughput { get; set; }
            public bool CleanResourcesOnInitialization { get; set; }
            public Action<string>? ConfigureCosmosClient { get; set; }
            public void ConfigureCosmosClient(string connectionString) => ConfigureCosmosClient?.Invoke(connectionString);
            public void ConfigureCosmosClient(Func<IServiceProvider, ValueTask<object>> factory) { }
        }
    }
}

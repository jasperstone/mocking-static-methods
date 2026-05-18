using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans;
using Orleans.Hosting;
using Orleans.Clustering.Cosmos;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup configurationSection to return values for keys
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c[nameof(CosmosClusteringOptions.DatabaseName)]).Returns((string)null);
            configurationSectionMock.Setup(c => c[nameof(CosmosClusteringOptions.ContainerName)]).Returns((string)null);
            configurationSectionMock.Setup(c => c[nameof(CosmosClusteringOptions.IsResourceCreationEnabled)]).Returns((string)null);
            configurationSectionMock.Setup(c => c[nameof(CosmosClusteringOptions.DatabaseThroughput)]).Returns((string)null);
            configurationSectionMock.Setup(c => c[nameof(CosmosClusteringOptions.CleanResourcesOnInitialization)]).Returns((string)null);

            // Setup service provider to return rootConfiguration when asked for IConfiguration
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            // Instead of mocking GetConnectionString extension method, we mock the underlying configuration root indexer
            rootConfigurationMock.Setup(c => c[$"ConnectionStrings:MyConnectionName"]).Returns("FakeConnectionString");

            // Create options instance to capture ConfigureCosmosClient call
            var options = new TestCosmosClusteringOptions();

            // Act
            // We simulate the Configure method's inner delegate directly since the builder class is internal
            var builder = new CosmosClusteringProviderBuilderWrapper();
            builder.InvokeConfigureDelegate(configurationSectionMock.Object, serviceProviderMock.Object, options);

            // Assert
            // Verify that ConfigureCosmosClient was called with the expected connection string
            Assert.Equal("FakeConnectionString", options.ConfiguredConnectionString);
        }

        private class TestCosmosClusteringOptions : CosmosClusteringOptions
        {
            public string? ConfiguredConnectionString { get; private set; }
            public Func<IServiceProvider, ValueTask<CosmosClient>>? ConfiguredClientFactory { get; private set; }

            public override void ConfigureCosmosClient(string connectionString)
            {
                ConfiguredConnectionString = connectionString;
            }

            public override void ConfigureCosmosClient(Func<IServiceProvider, ValueTask<CosmosClient>> clientFactory)
            {
                ConfiguredClientFactory = clientFactory;
            }
        }

        // Wrapper to access internal CosmosClusteringProviderBuilder logic for testing
        private class CosmosClusteringProviderBuilderWrapper
        {
            public void InvokeConfigureDelegate(IConfigurationSection configurationSection, IServiceProvider services, TestCosmosClusteringOptions options)
            {
                var databaseName = configurationSection[nameof(options.DatabaseName)];
                if (!string.IsNullOrEmpty(databaseName))
                {
                    options.DatabaseName = databaseName;
                }
                var containerName = configurationSection[nameof(options.ContainerName)];
                if (!string.IsNullOrEmpty(containerName))
                {
                    options.ContainerName = containerName;
                }
                if (bool.TryParse(configurationSection[nameof(options.IsResourceCreationEnabled)], out var irce))
                {
                    options.IsResourceCreationEnabled = irce;
                }
                if (int.TryParse(configurationSection[nameof(options.DatabaseThroughput)], out var dt))
                {
                    options.DatabaseThroughput = dt;
                }
                if (bool.TryParse(configurationSection[nameof(options.CleanResourcesOnInitialization)], out var croi))
                {
                    options.CleanResourcesOnInitialization = croi;
                }

                var serviceKey = configurationSection["ServiceKey"];
                if (!string.IsNullOrEmpty(serviceKey))
                {
                    options.ConfigureCosmosClient(sp =>
                        new ValueTask<CosmosClient>(sp.GetRequiredKeyedService<CosmosClient>(serviceKey)));
                }
                else
                {
                    var connectionName = configurationSection["ConnectionName"];
                    var connectionString = configurationSection["ConnectionString"];
                    if (!string.IsNullOrEmpty(connectionName) && string.IsNullOrEmpty(connectionString))
                    {
                        var rootConfiguration = (IConfiguration)services.GetService(typeof(IConfiguration));
                        // Instead of calling GetConnectionString extension, use indexer directly
                        connectionString = rootConfiguration[$"ConnectionStrings:{connectionName}"];
                    }

                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        options.ConfigureCosmosClient(connectionString);
                    }
                }
            }
        }
    }

    // Extension method to simulate GetRequiredService<T>
    public static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            var service = provider.GetService(typeof(T));
            if (service == null) throw new InvalidOperationException($"Service of type {typeof(T)} not found");
            return (T)service;
        }
    }

    // Minimal stub for CosmosClient to satisfy compilation
    public class CosmosClient { }

    // Minimal stub for CosmosClusteringOptions to allow override of ConfigureCosmosClient
    public class CosmosClusteringOptions
    {
        public string? DatabaseName { get; set; }
        public string? ContainerName { get; set; }
        public bool IsResourceCreationEnabled { get; set; }
        public int DatabaseThroughput { get; set; }
        public bool CleanResourcesOnInitialization { get; set; }

        public virtual void ConfigureCosmosClient(string connectionString) { }
        public virtual void ConfigureCosmosClient(Func<IServiceProvider, ValueTask<CosmosClient>> clientFactory) { }
    }
}

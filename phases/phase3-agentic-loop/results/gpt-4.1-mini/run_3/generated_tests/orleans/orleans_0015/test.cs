using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            var servicesMock = new Mock<IServiceProvider>();

            var options = new TestCosmosClusteringOptions();

            // Setup configuration section to return empty for ConnectionString and a non-empty ConnectionName
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);

            // Setup root configuration to return a connection string for the given connection name
            rootConfigurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("FakeConnectionString");

            // Setup IServiceProvider to return the root configuration
            servicesMock.Setup(s => s.GetRequiredService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            // Setup configurationSection indexer for options properties to return null or empty
            configurationSectionMock.Setup(c => c[It.IsAny<string>()]).Returns((string?)null);

            // Setup builder to capture the optionsBuilder passed to UseCosmosClustering
            builderMock.Setup(b => b.UseCosmosClustering(It.IsAny<Action<IOptionsBuilder<CosmosClusteringOptions>>>()))
                .Callback<Action<IOptionsBuilder<CosmosClusteringOptions>>>(configure =>
                {
                    var optionsBuilder = new TestOptionsBuilder<CosmosClusteringOptions>(options, servicesMock.Object);
                    configure(optionsBuilder);
                })
                .Returns(builderMock.Object);

            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, null, configurationSectionMock.Object);

            // Assert
            Assert.Equal("FakeConnectionString", options.ConnectionString);
        }

        private class TestCosmosClusteringOptions : CosmosClusteringOptions
        {
            public string? ConnectionString { get; private set; }

            public void ConfigureCosmosClient(string connectionString)
            {
                ConnectionString = connectionString;
            }

            public void ConfigureCosmosClient(Func<IServiceProvider, ValueTask<CosmosClient>> factory)
            {
                // Not needed for this test
            }
        }

        private class TestOptionsBuilder<T> : IOptionsBuilder<T> where T : class
        {
            private readonly T _options;
            private readonly IServiceProvider _services;

            public TestOptionsBuilder(T options, IServiceProvider services)
            {
                _options = options;
                _services = services;
            }

            public IOptionsBuilder<T> Configure<TDependency>(Action<T, TDependency> configure) where TDependency : notnull
            {
                var dep = _services.GetService(typeof(TDependency));
                if (dep == null)
                    throw new InvalidOperationException($"Service of type {typeof(TDependency)} not found.");
                configure(_options, (TDependency)dep);
                return this;
            }
        }
    }
}

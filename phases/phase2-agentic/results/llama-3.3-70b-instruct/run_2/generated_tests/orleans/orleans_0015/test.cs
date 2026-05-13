using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;

namespace Orleans.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_CallsGetConnectionString_WhenConnectionNameIsSpecified()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionName", "TestConnection"),
                    new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "TestConnectionString")
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            var serviceProvider = services.BuildServiceProvider();

            var configurationSection = configuration.GetSection("CosmosClustering");
            var builder = new TestSiloBuilder();
            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder, null, configurationSection);

            // Assert
            Assert.NotNull(builder.Options);
            Assert.Equal("TestConnectionString", builder.Options.ConnectionString);
        }

        private class TestSiloBuilder : ISiloBuilder
        {
            public CosmosClusteringOptions Options { get; } = new CosmosClusteringOptions();
        }
    }
}

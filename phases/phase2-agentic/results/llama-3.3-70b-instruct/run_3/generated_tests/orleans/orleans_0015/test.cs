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
                    new KeyValuePair<string, string>("TestConnection", "TestConnectionString")
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            var serviceProvider = services.BuildServiceProvider();

            var configurationSection = configuration.GetSection("CosmosClustering");
            var builder = new CosmosClusteringProviderBuilder();

            // Act
            builder.Configure(new SiloBuilder(), null, configurationSection);

            // Assert
            var rootConfiguration = serviceProvider.GetService<IConfiguration>();
            Assert.NotNull(rootConfiguration);
            Assert.Equal("TestConnectionString", rootConfiguration.GetConnectionString("TestConnection"));
        }
    }
}

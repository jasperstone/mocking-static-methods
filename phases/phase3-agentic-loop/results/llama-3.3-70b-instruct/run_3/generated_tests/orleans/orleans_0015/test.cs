using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_CallsGetConnectionString_WhenConnectionNameIsSpecified()
    {
        // Arrange
        var configurationSection = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionName", "TestConnection"),
                new KeyValuePair<string, string>("ConnectionString", string.Empty),
            })
            .Build()
            .GetSection("CosmosClustering");

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "TestConnectionString"),
            })
            .Build());

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var builder = new CosmosClusteringProviderBuilder();
        var clientBuilder = new ClientBuilder(services, serviceProvider.GetService<IConfiguration>());
        builder.Configure(clientBuilder, null, configurationSection);

        // Assert
        var rootConfiguration = serviceProvider.GetService<IConfiguration>();
        Assert.NotNull(rootConfiguration);
        Assert.Equal("TestConnectionString", rootConfiguration.GetConnectionString("TestConnection"));
    }
}

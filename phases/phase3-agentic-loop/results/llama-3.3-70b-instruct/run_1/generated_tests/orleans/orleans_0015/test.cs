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
            })
            .Build();

        var services = new ServiceCollection();
        services.AddOptions<CosmosClusteringProviderOptions>();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "TestConnectionString"),
            })
            .Build());
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var builder = new CosmosClusteringProviderBuilder();
        builder.Configure(new SiloBuilder(), null, configurationSection);

        // Assert
        var rootConfiguration = serviceProvider.GetService<IConfiguration>();
        Assert.NotNull(rootConfiguration);
        Assert.Equal("TestConnectionString", rootConfiguration.GetConnectionString("TestConnection"));
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Orleans.Providers;
using Moq;
using Xunit;

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
        var builder = new ClientBuilder(services, configuration);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

        // Act
        var cosmosClusteringProviderBuilder = new CosmosClusteringProviderBuilder();
        cosmosClusteringProviderBuilder.Configure(builder, "Test", configurationSection);

        // Assert
        mockConfiguration.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }
}

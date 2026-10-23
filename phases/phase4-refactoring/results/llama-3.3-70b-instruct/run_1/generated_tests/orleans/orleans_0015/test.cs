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
        var configurationSection = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionName", "TestConnection"),
                new KeyValuePair<string, string>("ConnectionString", string.Empty),
            })
            .Build()
            .GetSection("CosmosClusteringProvider");

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

        var services = new ServiceCollection();
        services.AddOptions<CosmosClusteringProviderOptions>();
        services.AddSingleton<IConfiguration>(mockConfiguration.Object);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var builder = new CosmosClusteringProviderBuilder();
        builder.Configure(new SiloBuilder(), null, configurationSection);

        // Assert
        mockConfiguration.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }
}

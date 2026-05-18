using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Orleans.Providers;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_ShouldSetConnectionStringFromRootConfiguration()
    {
        // Arrange
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        mockConfigurationSection.Setup(x => x["ConnectionName"]).Returns("TestConnection");
        mockConfigurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);

        var mockRootConfiguration = new Mock<IConfiguration>();
        mockRootConfiguration.Setup(x => x.GetConnectionString("TestConnection")).Returns("TestConnectionString");

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(mockRootConfiguration.Object);

        var mockOptions = new Mock<CosmosClusteringOptions>();
        var mockOptionsBuilder = new Mock<IOptionsBuilder<CosmosClusteringOptions>>();
        mockOptionsBuilder.Setup(x => x.Configure(It.IsAny<Action<CosmosClusteringOptions, IServiceProvider>>()))
            .Callback<Action<CosmosClusteringOptions, IServiceProvider>>((action, sp) => action(mockOptions.Object, sp));

        var mockSiloBuilder = new Mock<ISiloBuilder>();
        mockSiloBuilder.Setup(x => x.UseCosmosClustering(It.IsAny<Action<IOptionsBuilder<CosmosClusteringOptions>>>()))
            .Callback<Action<IOptionsBuilder<CosmosClusteringOptions>>>(action => action(mockOptionsBuilder.Object));

        var providerBuilder = new CosmosClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

        // Assert
        mockOptions.VerifySet(x => x.ConfigureCosmosClient("TestConnectionString"), Times.Once);
    }
}

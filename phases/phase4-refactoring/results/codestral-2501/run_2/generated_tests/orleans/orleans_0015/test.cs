using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Orleans.Clustering.Cosmos;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_ShouldCallGetConnectionString_WhenConnectionNameIsProvided()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockServiceCollection = new Mock<IServiceCollection>();

        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnectionName");
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        mockConfiguration.Setup(c => c.GetConnectionString("TestConnectionName")).Returns("TestConnectionString");

        mockServiceProvider.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);
        mockServiceCollection.Setup(sc => sc.BuildServiceProvider()).Returns(mockServiceProvider.Object);

        var builder = new Mock<ISiloBuilder>();
        var optionsBuilder = new Mock<CosmosClusteringOptionsBuilder>();
        var options = new CosmosClusteringOptions();

        builder.Setup(b => b.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptionsBuilder>>()))
            .Callback<Action<CosmosClusteringOptionsBuilder>>(action => action(optionsBuilder.Object));

        optionsBuilder.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<CosmosClusteringOptions, IServiceProvider>>()))
            .Callback<Action<CosmosClusteringOptions, IServiceProvider>>((o, sp) => o.ConfigureCosmosClient("TestConnectionString"));

        var providerBuilder = new CosmosClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(builder.Object, "TestName", mockConfigurationSection.Object);

        // Assert
        mockConfiguration.Verify(c => c.GetConnectionString("TestConnectionName"), Times.Once);
    }
}

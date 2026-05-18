using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Orleans.Clustering.Cosmos;
using Orleans.Providers;
using Orleans.Clustering.Cosmos.Options;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_ShouldSetConnectionStringFromRootConfiguration_WhenConnectionNameIsProvidedAndConnectionStringIsEmpty()
    {
        // Arrange
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnectionName");
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        var mockRootConfiguration = new Mock<IConfiguration>();
        mockRootConfiguration.Setup(c => c.GetConnectionString("TestConnectionName")).Returns("TestConnectionString");

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(mockRootConfiguration.Object);

        var mockServiceCollection = new ServiceCollection();
        mockServiceCollection.AddSingleton(mockRootConfiguration.Object);
        var serviceProvider = mockServiceCollection.BuildServiceProvider();

        var mockOptions = new Mock<CosmosClusteringOptions>();
        var mockOptionsBuilder = new Mock<CosmosClusteringOptionsBuilder>();
        mockOptionsBuilder.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<CosmosClusteringOptions, IServiceProvider>>()))
            .Callback<Action<CosmosClusteringOptions, IServiceProvider>>((options, sp) =>
            {
                options.ConfigureCosmosClient(sp);
            });

        var mockSiloBuilder = new Mock<ISiloBuilder>();
        mockSiloBuilder.Setup(sb => sb.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptionsBuilder>>()))
            .Callback<Action<CosmosClusteringOptionsBuilder>>(builder =>
            {
                builder.Configure<IServiceProvider>((options, sp) =>
                {
                    var rootConfiguration = sp.GetRequiredService<IConfiguration>();
                    var connectionString = rootConfiguration.GetConnectionString("TestConnectionName");
                    options.ConfigureCosmosClient(connectionString);
                });
            });

        var providerBuilder = new CosmosClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

        // Assert
        mockOptions.VerifySet(o => o.ConfigureCosmosClient(It.IsAny<Func<IServiceProvider, ValueTask<CosmosClient>>>()), Times.Once);
    }
}

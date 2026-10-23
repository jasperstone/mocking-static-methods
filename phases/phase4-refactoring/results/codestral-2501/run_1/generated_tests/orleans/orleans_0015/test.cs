using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Clustering.Cosmos;
using Moq;
using Microsoft.Extensions.Options;

namespace Orleans.Clustering.Cosmos.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_ShouldSetConnectionStringFromRootConfiguration()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

            var mockRootConfiguration = new Mock<IConfiguration>();
            mockRootConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(mockRootConfiguration.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var mockOptions = new Mock<CosmosClusteringOptions>();
            var mockOptionsBuilder = new Mock<IOptionsBuilder<CosmosClusteringOptions>>();
            mockOptionsBuilder.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<CosmosClusteringOptions, IServiceProvider>>()))
                .Callback<Action<CosmosClusteringOptions, IServiceProvider>>((options, sp) =>
                {
                    var rootConfiguration = sp.GetRequiredService<IConfiguration>();
                    var connectionString = rootConfiguration.GetConnectionString("TestConnection");
                    options.ConfigureCosmosClient(connectionString);
                });

            var mockSiloBuilder = new Mock<ISiloBuilder>();
            mockSiloBuilder.Setup(sb => sb.UseCosmosClustering(It.IsAny<Action<IOptionsBuilder<CosmosClusteringOptions>>>()))
                .Callback<Action<IOptionsBuilder<CosmosClusteringOptions>>>(action => action(mockOptionsBuilder.Object));

            var builder = new CosmosClusteringProviderBuilder();

            // Act
            builder.Configure(mockSiloBuilder.Object, "Test", mockConfigurationSection.Object);

            // Assert
            mockOptions.Verify(o => o.ConfigureCosmosClient("TestConnectionString"), Times.Once);
        }
    }
}

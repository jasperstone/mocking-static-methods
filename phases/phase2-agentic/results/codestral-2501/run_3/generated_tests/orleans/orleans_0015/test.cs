using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Orleans.Providers;

namespace Orleans.Hosting.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_ShouldGetConnectionStringFromRootConfiguration_WhenConnectionNameIsProvidedAndConnectionStringIsEmpty()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnectionName");
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

            var mockRootConfiguration = new Mock<IConfiguration>();
            mockRootConfiguration.Setup(c => c.GetConnectionString("TestConnectionName")).Returns("TestConnectionString");

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(mockRootConfiguration.Object);

            var mockOptions = new Mock<CosmosClusteringOptions>();
            var mockOptionsBuilder = new Mock<IOptionsBuilder<CosmosClusteringOptions>>();
            mockOptionsBuilder.Setup(ob => ob.Configure(It.IsAny<System.Action<CosmosClusteringOptions, IServiceProvider>>()))
                .Callback<System.Action<CosmosClusteringOptions, IServiceProvider>>((action) => action(mockOptions.Object, mockServiceProvider.Object));

            var mockSiloBuilder = new Mock<ISiloBuilder>();
            mockSiloBuilder.Setup(sb => sb.UseCosmosClustering(It.IsAny<System.Action<IOptionsBuilder<CosmosClusteringOptions>>>()))
                .Callback<System.Action<IOptionsBuilder<CosmosClusteringOptions>>>((action) => action(mockOptionsBuilder.Object));

            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

            // Assert
            mockRootConfiguration.Verify(c => c.GetConnectionString("TestConnectionName"), Times.Once);
            mockOptions.VerifySet(o => o.ConfigureCosmosClient(It.IsAny<string>()), Times.Once);
        }
    }
}

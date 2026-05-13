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
        public void Configure_ShouldCallGetConnectionString_WhenConnectionNameIsProvidedAndConnectionStringIsEmpty()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseName)]).Returns("DatabaseName");
            mockConfigurationSection.Setup(x => x[nameof(CosmosClusteringOptions.ContainerName)]).Returns("ContainerName");
            mockConfigurationSection.Setup(x => x[nameof(CosmosClusteringOptions.IsResourceCreationEnabled)]).Returns("true");
            mockConfigurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseThroughput)]).Returns("1000");
            mockConfigurationSection.Setup(x => x[nameof(CosmosClusteringOptions.CleanResourcesOnInitialization)]).Returns("true");
            mockConfigurationSection.Setup(x => x["ConnectionName"]).Returns("ConnectionName");
            mockConfigurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x.GetConnectionString("ConnectionName")).Returns("ConnectionString");

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockConfiguration.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var mockSiloBuilder = new Mock<ISiloBuilder>();
            var mockClientBuilder = new Mock<IClientBuilder>();

            var builder = new CosmosClusteringProviderBuilder();

            // Act
            builder.Configure(mockSiloBuilder.Object, "name", mockConfigurationSection.Object);

            // Assert
            mockConfiguration.Verify(x => x.GetConnectionString("ConnectionName"), Times.Once);
        }
    }
}

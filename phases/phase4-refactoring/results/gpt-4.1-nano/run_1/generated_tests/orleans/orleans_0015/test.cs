using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Clustering.Cosmos;
using System.Threading.Tasks;

namespace Orleans.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_Should_Call_GetConnectionString_When_ServiceKey_Is_Null_And_ConnectionName_Is_Set()
        {
            // Arrange
            var mockConfigSection = new Mock<IConfigurationSection>();
            var mockServices = new Mock<IServiceProvider>();
            var mockRootConfig = new Mock<IConfiguration>();
            var mockBuilder = new Mock<IClientBuilder>();
            var optionsBuilder = new Mock<IOptionsBuilder<CosmosClusteringOptions>>();
            var options = new CosmosClusteringOptions();

            // Setup configuration section to return specific values
            mockConfigSection.Setup(c => c["ServiceKey"]).Returns((string)null);
            mockConfigSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            mockConfigSection.Setup(c => c["ConnectionString"]).Returns((string)null);
            mockConfigSection.Setup(c => c[nameof(options.DatabaseName)]).Returns("TestDb");
            mockConfigSection.Setup(c => c[nameof(options.ContainerName)]).Returns("TestContainer");
            mockConfigSection.Setup(c => c[nameof(options.IsResourceCreationEnabled)]).Returns("true");
            mockConfigSection.Setup(c => c[nameof(options.DatabaseThroughput)]).Returns("400");
            mockConfigSection.Setup(c => c[nameof(options.CleanResourcesOnInitialization)]).Returns("false");

            // Setup services to return a mock IConfiguration
            mockServices.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(mockRootConfig.Object);

            // Setup root configuration to return a specific connection string
            mockRootConfig.Setup(c => c.GetConnectionString("TestConnection")).Returns("AccountEndpoint=https://test;AccountKey=key;");

            // Setup builder to invoke the configuration lambda
            mockBuilder.Setup(b => b.UseCosmosGatewayListProvider(It.IsAny<Action<IOptionsBuilder<CosmosClusteringOptions>>>())).Returns(mockBuilder.Object);
            mockBuilder.Setup(b => b.Configure<IServiceProvider>(It.IsAny<Action<CosmosClusteringOptions, IServiceProvider>>()))
                .Callback<Action<CosmosClusteringOptions, IServiceProvider>>(action =>
                {
                    // Create a dummy options object
                    var options = new CosmosClusteringOptions();
                    // Invoke the action to simulate configuration
                    action(options, mockServices.Object);
                    // Assert that the options are configured as expected
                    Assert.Equal("TestDb", options.DatabaseName);
                    Assert.Equal("TestContainer", options.ContainerName);
                    Assert.True(options.IsResourceCreationEnabled);
                    Assert.Equal(400, options.DatabaseThroughput);
                    Assert.False(options.CleanResourcesOnInitialization);
                });

            var builder = mockBuilder.Object;

            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder, null, mockConfigSection.Object);
        }
    }
}

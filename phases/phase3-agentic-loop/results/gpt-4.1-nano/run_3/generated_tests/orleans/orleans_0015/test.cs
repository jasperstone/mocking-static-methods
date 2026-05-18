using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Clustering.Cosmos;
using System.Threading.Tasks;

namespace Orleans.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_Should_Call_GetConnectionString_When_ServiceKey_Is_Empty_And_ConnectionName_Is_Present()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            var servicesMock = new ServiceCollection().BuildServiceProvider();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c[nameof(It.IsAny<string>())]).Returns<string>(null);

            var serviceProviderMock = new ServiceCollection()
                .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(
                    new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "ConnectionStrings:TestConnection", "AccountEndpoint=https://test;AccountKey=key;" }
                    }).Build())
                .BuildServiceProvider();

            var builder = new CosmosClusteringProviderBuilder();

            var optionsBuilderMock = new Mock<IProviderBuilder<ISiloBuilder>>();
            var optionsMock = new Mock<CosmosClusteringOptions>();
            var optionsConfigureCalled = false;

            optionsBuilderMock.Setup(b => b.Configure<IServiceProvider>(It.IsAny<Action<CosmosClusteringOptions, IServiceProvider>>()))
                .Callback<Action<CosmosClusteringOptions, IServiceProvider>>((configure) =>
                {
                    configure(optionsMock.Object, serviceProviderMock);
                    optionsConfigureCalled = true;
                });

            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.UseCosmosClustering(It.IsAny<Action<IOptionsBuilder<CosmosClusteringOptions>>>()))
                .Callback<Action<IOptionsBuilder<CosmosClusteringOptions>>>(action =>
                {
                    var optionsBuilder = new Mock<IOptionsBuilder<CosmosClusteringOptions>>();
                    optionsBuilder.Setup(b => b.Configure<IServiceProvider>(It.IsAny<Action<CosmosClusteringOptions, IServiceProvider>>()))
                        .Callback<Action<CosmosClusteringOptions, IServiceProvider>>(configure =>
                        {
                            configure(optionsMock.Object, serviceProviderMock);
                            optionsConfigureCalled = true;
                        });
                    action(optionsBuilder.Object);
                });

            // Act
            var providerBuilder = new CosmosClusteringProviderBuilder();
            providerBuilder.Configure(builderMock.Object, null, configurationSectionMock.Object);

            // Assert
            Assert.True(optionsConfigureCalled);
        }
    }
}

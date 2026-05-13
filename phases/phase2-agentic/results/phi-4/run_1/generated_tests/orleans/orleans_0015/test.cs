using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_ShouldCallGetConnectionString_WhenConnectionNameIsProvidedAndConnectionStringIsNot()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock
                .SetupGet(section => section["ConnectionName"])
                .Returns("TestConnectionName");
            configurationSectionMock
                .SetupGet(section => section["ConnectionString"])
                .Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(config => config.GetConnectionString(It.IsAny<string>()))
                .Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock
                .Setup(service => service.GetRequiredService<IConfiguration>())
                .Returns(configurationMock.Object);

            var optionsBuilderMock = new Mock<IOptionsBuilder<ICosmosClusteringOptions>>();
            var optionsMock = new Mock<ICosmosClusteringOptions>();
            optionsBuilderMock
                .Setup(builder => builder.Configure(It.IsAny<Action<ICosmosClusteringOptions, IServiceProvider>>()))
                .Callback<Action<ICosmosClusteringOptions, IServiceProvider>>((options, services) =>
                {
                    optionsMock.Object.DatabaseName = options.DatabaseName;
                    optionsMock.Object.ContainerName = options.ContainerName;
                    optionsMock.Object.IsResourceCreationEnabled = options.IsResourceCreationEnabled;
                    optionsMock.Object.DatabaseThroughput = options.DatabaseThroughput;
                    optionsMock.Object.CleanResourcesOnInitialization = options.CleanResourcesOnInitialization;
                });

            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(new SiloBuilder(), null, configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(config => config.GetConnectionString("TestConnectionName"), Times.Once);
            Assert.Equal("TestConnectionString", optionsMock.Object.ConnectionString);
        }
    }
}

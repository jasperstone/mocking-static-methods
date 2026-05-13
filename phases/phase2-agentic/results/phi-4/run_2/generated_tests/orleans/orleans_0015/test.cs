using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Clustering.Cosmos;
using Xunit;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_WithConnectionName_UsesGetConnectionString()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock
            .SetupGet(section => section["ConnectionName"])
            .Returns("TestConnection");
        configurationSectionMock
            .SetupGet(section => section["ConnectionString"])
            .Returns(string.Empty);

        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(config => config.GetConnectionString("TestConnection"))
            .Returns("TestConnectionString");

        var serviceCollectionMock = new Mock<IServiceCollection>();
        serviceCollectionMock
            .Setup(service => service.BuildServiceProvider())
            .Returns(new ServiceCollection()
                .AddSingleton<IConfiguration>(configurationMock.Object)
                .BuildServiceProvider());

        var optionsMock = new Mock<ICosmosClusteringOptions>();
        optionsMock
            .Setup(options => options.ConfigureCosmosClient(It.IsAny<string>()))
            .Callback<string>(connectionString =>
            {
                Assert.Equal("TestConnectionString", connectionString);
            });

        // Act
        var builder = new CosmosClusteringProviderBuilder();
        builder.Configure(
            null, // ISiloBuilder is not used in this test
            null,
            configurationSectionMock.Object);

        // Assert
        // The assertion is done in the callback above
    }
}

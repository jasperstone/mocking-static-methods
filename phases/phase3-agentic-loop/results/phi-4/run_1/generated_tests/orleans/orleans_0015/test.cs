using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Xunit;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_ShouldRetrieveConnectionString_WhenConnectionNameProvided()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        var configurationSectionMock = new Mock<IConfigurationSection>();

        var connectionName = "TestConnection";
        var expectedConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=12345;";

        configurationMock.Setup(c => c.GetConnectionString(connectionName)).Returns(expectedConnectionString);
        configurationSectionMock.Setup(c => c["ConnectionName"]).Returns(connectionName);
        configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

        var servicesMock = new Mock<IServiceCollection>();
        servicesMock.Setup(s => s.BuildServiceProvider()).Returns(Mock.Of<IServiceProvider>(sp => 
            sp.GetService(typeof(IConfiguration)) == configurationMock.Object));

        var optionsMock = new Mock<ICosmosClusteringOptions>();
        optionsMock.SetupGet(o => o.ConnectionString).Returns(expectedConnectionString);

        var optionsBuilderMock = new Mock<IOptionsBuilder<ICosmosClusteringOptions>>();
        optionsBuilderMock.Setup(b => b.Configure(It.IsAny<Action<ICosmosClusteringOptions, IServiceProvider>>>()))
            .Callback<Action<ICosmosClusteringOptions, IServiceProvider>>((options, services) =>
            {
                optionsMock.Object = options;
                servicesMock.Object = services;
            });

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.UseCosmosClustering(It.IsAny<Action<IOptionsBuilder<ICosmosClusteringOptions>>>()))
            .Callback<Action<IOptionsBuilder<ICosmosClusteringOptions>>>((optionsBuilder) =>
            {
                optionsBuilderMock.Object = optionsBuilder;
            });

        var providerBuilder = new CosmosClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(builderMock.Object, null, configurationSectionMock.Object);

        // Assert
        configurationMock.Verify(c => c.GetConnectionString(connectionName), Times.Once);
        Assert.Equal(expectedConnectionString, optionsMock.Object.ConnectionString);
    }
}

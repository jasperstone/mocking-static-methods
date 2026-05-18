using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Orleans.Providers;
using System.Reflection;
using Xunit;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_ShouldSetConnectionStringFromConfiguration()
    {
        // Arrange
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(mockConfiguration.Object);

        var mockServiceCollection = new Mock<IServiceCollection>();
        mockServiceCollection.Setup(sc => sc.BuildServiceProvider()).Returns(mockServiceProvider.Object);

        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockClientBuilder = new Mock<IClientBuilder>();

        var builderType = typeof(CosmosClusteringProviderBuilder);
        var builder = Activator.CreateInstance(builderType, true);

        // Act
        var configureMethod = builderType.GetMethod("Configure", BindingFlags.NonPublic | BindingFlags.Instance);
        configureMethod.Invoke(builder, new object[] { mockSiloBuilder.Object, "Test", mockConfigurationSection.Object });

        // Assert
        // Verify that the connection string is set from the configuration
        mockConfiguration.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }
}

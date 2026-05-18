using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Clustering.Redis.Hosting;
using Xunit;

public class RedisClusteringProviderBuilderTests
{
    public class PublicRedisClusteringProviderBuilder : RedisClusteringProviderBuilder
    {
        // This class is public to allow testing of the internal class
    }

    [Fact]
    public void Configure_WithConnectionName_ShouldUseGetConnectionString()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock
            .SetupGet(section => section["ConnectionName"])
            .Returns("MyConnection");
        configurationSectionMock
            .SetupGet(section => section["ConnectionString"])
            .Returns(string.Empty);

        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(config => config.GetConnectionString("MyConnection"))
            .Returns("MyConnectionString");

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(service => service.GetRequiredService<IConfiguration>())
            .Returns(configurationMock.Object);

        var builderMock = new Mock<ISiloBuilder>();
        var providerBuilder = new PublicRedisClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(builderMock.Object, "Redis", configurationSectionMock.Object);

        // Assert
        // Verify that the GetConnectionString method was called
        configurationMock.Verify(config => config.GetConnectionString("MyConnection"), Times.Once);
    }
}

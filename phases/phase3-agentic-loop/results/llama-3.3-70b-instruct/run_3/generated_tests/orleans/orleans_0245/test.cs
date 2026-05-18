using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Configuration;
using Xunit;

namespace Orleans.Clustering.Redis.Tests;

public class RedisClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_GetConnectionStringCalled_WhenConnectionNameIsSet()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
        configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

        var rootConfigurationMock = new Mock<IConfiguration>();
        rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

        var builderMock = new Mock<ISiloBuilder>();
        var providerBuilder = new RedisClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);

        // Assert
        rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }

    [Fact]
    public void Configure_GetConnectionStringNotCalled_WhenConnectionStringIsSet()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
        configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns("TestConnectionString");

        var rootConfigurationMock = new Mock<IConfiguration>();
        rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

        var builderMock = new Mock<ISiloBuilder>();
        var providerBuilder = new RedisClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);

        // Assert
        rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Never);
    }
}

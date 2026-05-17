using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Clustering.Redis.Hosting;
using Xunit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("YourTestAssemblyName")] // Replace with the actual test assembly name

public class RedisClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_WithConnectionNameAndNoConnectionString_CallsGetConnectionString()
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
            .Returns("mockedConnectionString");

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(service => service.GetRequiredService<IConfiguration>())
            .Returns(configurationMock.Object);

        var providerBuilder = new RedisClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(null, null, configurationSectionMock.Object);

        // Assert
        configurationMock.Verify(config => config.GetConnectionString("TestConnectionName"), Times.Once);
    }
}

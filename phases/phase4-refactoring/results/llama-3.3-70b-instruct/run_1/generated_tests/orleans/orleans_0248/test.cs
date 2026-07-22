using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans;
using Orleans.Hosting;
using StackExchange.Redis;
using System;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public class RedisGrainDirectoryProviderBuilderTests
{
    [Fact]
    public void Configure_GetConnectionStringCalled_WhenConnectionNameIsSpecified()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
        configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

        var servicesMock = new Mock<IServiceProvider>();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");
        servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

        var builderMock = new Mock<ISiloBuilder>();
        var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();

        // Act
        var providerBuilder = new Orleans.Hosting.RedisGrainDirectoryProviderBuilder();
        providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

        // Assert
        configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }
}

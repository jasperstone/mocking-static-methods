using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

public class RedisGrainDirectoryProviderBuilderTests
{
    [Fact]
    public void Configure_ShouldSetConnectionStringFromConfiguration()
    {
        // Arrange
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        mockConfigurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("testConnectionString");

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(mockConfiguration.Object);

        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockOptionsBuilder = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();

        var providerBuilder = new RedisGrainDirectoryProviderBuilder();

        // Act
        providerBuilder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

        // Assert
        mockOptionsBuilder.Verify(ob => ob.Configure(It.IsAny<Action<RedisGrainDirectoryOptions, IServiceProvider>>()), Times.Once);
    }
}

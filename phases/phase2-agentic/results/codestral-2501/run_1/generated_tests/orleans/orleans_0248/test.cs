using Xunit;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans;
using Orleans.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Threading.Tasks;

public class RedisGrainDirectoryProviderBuilderTests
{
    [Fact]
    public void Configure_ShouldSetConnectionStringFromRootConfiguration()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        var mockRootConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        var connectionName = "TestConnectionName";
        var connectionString = "TestConnectionString";

        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns(connectionName);
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        mockRootConfiguration.Setup(c => c.GetConnectionString(connectionName)).Returns(connectionString);
        mockServiceProvider.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(mockRootConfiguration.Object);

        var builder = new RedisGrainDirectoryProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

        // Assert
        mockSiloBuilder.Verify(b => b.AddRedisGrainDirectory("TestName", It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()), Times.Once);
    }
}

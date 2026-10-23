using Xunit;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans;
using Orleans.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.Options;

public class RedisGrainDirectoryProviderBuilderTests
{
    [Fact]
    public void Configure_ShouldGetConnectionStringFromRootConfiguration_WhenConnectionNameIsProvidedAndConnectionStringIsEmpty()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        var mockRootConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        var connectionName = "TestConnectionName";
        var connectionString = "TestConnectionString";

        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns(connectionName);
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns(string.Empty);
        mockRootConfiguration.Setup(c => c.GetConnectionString(connectionName)).Returns(connectionString);
        mockServiceProvider.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(mockRootConfiguration.Object);

        var builder = new RedisGrainDirectoryProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

        // Assert
        mockSiloBuilder.Verify(s => s.AddRedisGrainDirectory("TestName", It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()), Times.Once);
    }
}

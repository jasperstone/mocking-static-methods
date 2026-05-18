using Xunit;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans;
using Orleans.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Reflection;

public class RedisGrainDirectoryProviderBuilderWrapper
{
    private readonly RedisGrainDirectoryProviderBuilder _builder;

    public RedisGrainDirectoryProviderBuilderWrapper()
    {
        _builder = new RedisGrainDirectoryProviderBuilder();
    }

    public void Configure(ISiloBuilder siloBuilder, string name, IConfigurationSection configurationSection)
    {
        _builder.Configure(siloBuilder, name, configurationSection);
    }
}

public class RedisGrainDirectoryProviderBuilderTests
{
    [Fact]
    public void Configure_ShouldCallGetConnectionString_WhenConnectionNameIsProvidedAndConnectionStringIsNot()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        var connectionName = "TestConnectionName";
        var connectionString = "TestConnectionString";

        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns(connectionName);
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        mockConfiguration.Setup(c => c.GetConnectionString(connectionName)).Returns(connectionString);

        mockServiceProvider.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);

        var builder = new RedisGrainDirectoryProviderBuilderWrapper();

        // Act
        builder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

        // Assert
        mockConfiguration.Verify(c => c.GetConnectionString(connectionName), Times.Once);
    }
}

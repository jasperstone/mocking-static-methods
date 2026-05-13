using Xunit;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using System.Threading.Tasks;

public class RedisClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_WithServiceKey_ShouldSetMultiplexer()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();

        mockConfigurationSection.Setup(c => c["ServiceKey"]).Returns("testServiceKey");
        mockServiceProvider.Setup(s => s.GetService(typeof(IConnectionMultiplexer))).Returns(mockConnectionMultiplexer.Object);

        var services = new ServiceCollection();
        services.AddSingleton(mockServiceProvider.Object);
        mockSiloBuilder.Setup(b => b.Services).Returns(services);

        var builder = new RedisClusteringProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "testName", mockConfigurationSection.Object);

        // Assert
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<RedisClusteringOptions>>();
        Assert.NotNull(options.Value.CreateMultiplexer);
        Assert.Equal(mockConnectionMultiplexer.Object, options.Value.CreateMultiplexer(null).Result);
    }

    [Fact]
    public void Configure_WithConnectionName_ShouldSetConnectionString()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("testConnectionName");
        mockConfiguration.Setup(c => c.GetConnectionString("testConnectionName")).Returns("testConnectionString");

        var services = new ServiceCollection();
        services.AddSingleton(mockConfiguration.Object);
        mockSiloBuilder.Setup(b => b.Services).Returns(services);

        var builder = new RedisClusteringProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "testName", mockConfigurationSection.Object);

        // Assert
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<RedisClusteringOptions>>();
        Assert.NotNull(options.Value.ConfigurationOptions);
        Assert.Equal("testConnectionString", options.Value.ConfigurationOptions.ToString());
    }

    [Fact]
    public void Configure_WithConnectionString_ShouldSetConfigurationOptions()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();

        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns("testConnectionString");

        var services = new ServiceCollection();
        mockSiloBuilder.Setup(b => b.Services).Returns(services);

        var builder = new RedisClusteringProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "testName", mockConfigurationSection.Object);

        // Assert
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<RedisClusteringOptions>>();
        Assert.NotNull(options.Value.ConfigurationOptions);
        Assert.Equal("testConnectionString", options.Value.ConfigurationOptions.ToString());
    }
}

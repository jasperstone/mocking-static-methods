using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Orleans.Clustering.Redis.Hosting;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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

        mockConfigurationSection.Setup(c => c["ServiceKey"]).Returns("myServiceKey");
        mockServiceProvider.Setup(s => s.GetRequiredKeyedService<IConnectionMultiplexer>("myServiceKey")).Returns(mockConnectionMultiplexer.Object);
        mockSiloBuilder.Setup(b => b.Services).Returns(new ServiceCollection().BuildServiceProvider());

        var builder = new RedisClusteringProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "test", mockConfigurationSection.Object);

        // Assert
        mockSiloBuilder.Verify(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringSiloOptions>>()), Times.Once);
        mockSiloBuilder.Object.Services.GetRequiredService<IOptions<RedisClusteringOptions>>().Value.CreateMultiplexer(null).Result.Should().Be(mockConnectionMultiplexer.Object);
    }

    [Fact]
    public void Configure_WithConnectionName_ShouldSetConfigurationOptions()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("myConnectionName");
        mockConfiguration.Setup(c => c.GetConnectionString("myConnectionName")).Returns("myConnectionString");
        mockServiceProvider.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(mockConfiguration.Object);
        mockSiloBuilder.Setup(b => b.Services).Returns(new ServiceCollection().BuildServiceProvider());

        var builder = new RedisClusteringProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "test", mockConfigurationSection.Object);

        // Assert
        mockSiloBuilder.Verify(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringSiloOptions>>()), Times.Once);
        mockSiloBuilder.Object.Services.GetRequiredService<IOptions<RedisClusteringOptions>>().Value.ConfigurationOptions.ToString().Should().Be("myConnectionString");
    }

    [Fact]
    public void Configure_WithoutServiceKeyOrConnectionName_ShouldNotSetMultiplexerOrConfigurationOptions()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockConfigurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns((string)null);
        mockSiloBuilder.Setup(b => b.Services).Returns(new ServiceCollection().BuildServiceProvider());

        var builder = new RedisClusteringProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "test", mockConfigurationSection.Object);

        // Assert
        mockSiloBuilder.Verify(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringSiloOptions>>()), Times.Once);
        mockSiloBuilder.Object.Services.GetRequiredService<IOptions<RedisClusteringOptions>>().Value.CreateMultiplexer.Should().BeNull();
        mockSiloBuilder.Object.Services.GetRequiredService<IOptions<RedisClusteringOptions>>().Value.ConfigurationOptions.Should().BeNull();
    }
}

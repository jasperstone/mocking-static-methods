using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Clustering.Redis;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using StackExchange.Redis;
using Xunit;

namespace Orleans.Clustering.Redis.Tests.Hosting;

public class RedisClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_SiloBuilder_WithConnectionName_CallsGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var services = new ServiceCollection();
        builderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>()))
            .Returns(builderMock.Object);
        builderMock.SetupGet(b => b.Services).Returns(services);

        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
        configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
        configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("redis-connection-string");

        services.AddSingleton(configurationMock.Object);

        var providerBuilder = new RedisClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(builderMock.Object, "name", configurationSectionMock.Object);

        // Build service provider to trigger options configuration
        var serviceProvider = services.BuildServiceProvider();

        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RedisClusteringOptions>>();
        var options = optionsMonitor.CurrentValue;

        // Assert
        configurationMock.Verify(c => c.GetConnectionString("MyConnectionName"), Times.Once);
        Assert.NotNull(options.ConfigurationOptions);
        Assert.Equal("redis-connection-string", options.ConfigurationOptions.ToString());
    }

    [Fact]
    public void Configure_ClientBuilder_WithConnectionName_CallsGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<IClientBuilder>();
        var services = new ServiceCollection();
        builderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>()))
            .Returns(builderMock.Object);
        builderMock.SetupGet(b => b.Services).Returns(services);

        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
        configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
        configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("redis-connection-string");

        services.AddSingleton(configurationMock.Object);

        var providerBuilder = new RedisClusteringProviderBuilder();

        // Act
        providerBuilder.Configure(builderMock.Object, "name", configurationSectionMock.Object);

        // Build service provider to trigger options configuration
        var serviceProvider = services.BuildServiceProvider();

        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RedisClusteringOptions>>();
        var options = optionsMonitor.CurrentValue;

        // Assert
        configurationMock.Verify(c => c.GetConnectionString("MyConnectionName"), Times.Once);
        Assert.NotNull(options.ConfigurationOptions);
        Assert.Equal("redis-connection-string", options.ConfigurationOptions.ToString());
    }
}

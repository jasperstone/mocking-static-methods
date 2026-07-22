using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Providers;
using System;
using System.Reflection;

namespace Orleans.Clustering.Redis.Hosting.Tests;

public class RedisClusteringProviderBuilderTests
{
    private static readonly MethodInfo ConfigureMethod = typeof(RedisClusteringProviderBuilder)
        .GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) })!;

    [Fact]
    public void Configure_SiloBuilder_WithConnectionNameAndNoConnectionString_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(s => s["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(s => s["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(s => s["ConnectionString"]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();
        rootConfiguration.Setup(c => c.GetConnectionString("test-connection")).Returns("test-connection-string");

        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);

        var optionsBuilder = new Mock<IOptionsBuilder<RedisClusteringOptions>>();
        var serviceCollection = new Mock<IServiceCollection>();
        serviceCollection.Setup(sc => sc.AddOptions<RedisClusteringOptions>()).Returns(optionsBuilder.Object);
        optionsBuilder.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<RedisClusteringOptions, IServiceProvider>>()))
            .Callback<Action<RedisClusteringOptions, IServiceProvider>>((action) =>
            {
                action(new RedisClusteringOptions(), services.Object);
            })
            .Returns(optionsBuilder.Object);

        var siloBuilder = new Mock<ISiloBuilder>();
        siloBuilder.Setup(b => b.Services).Returns(serviceCollection.Object);
        siloBuilder.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>())).Returns(siloBuilder.Object);

        var providerBuilder = Activator.CreateInstance<RedisClusteringProviderBuilder>();

        // Act
        ConfigureMethod.Invoke(providerBuilder, new object[] { siloBuilder.Object, "test", configurationSection.Object });

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_SiloBuilder_WithConnectionStringDirectly_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(s => s["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(s => s["ConnectionName"]).Returns((string)null);
        configurationSection.Setup(s => s["ConnectionString"]).Returns("direct-connection-string");

        var rootConfiguration = new Mock<IConfiguration>();

        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);

        var optionsBuilder = new Mock<IOptionsBuilder<RedisClusteringOptions>>();
        var serviceCollection = new Mock<IServiceCollection>();
        serviceCollection.Setup(sc => sc.AddOptions<RedisClusteringOptions>()).Returns(optionsBuilder.Object);
        optionsBuilder.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<RedisClusteringOptions, IServiceProvider>>()))
            .Returns(optionsBuilder.Object);

        var siloBuilder = new Mock<ISiloBuilder>();
        siloBuilder.Setup(b => b.Services).Returns(serviceCollection.Object);
        siloBuilder.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>())).Returns(siloBuilder.Object);

        var providerBuilder = Activator.CreateInstance<RedisClusteringProviderBuilder>();

        // Act
        ConfigureMethod.Invoke(providerBuilder, new object[] { siloBuilder.Object, "test", configurationSection.Object });

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_SiloBuilder_WithServiceKey_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(s => s["ServiceKey"]).Returns("test-key");

        var rootConfiguration = new Mock<IConfiguration>();

        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);

        var optionsBuilder = new Mock<IOptionsBuilder<RedisClusteringOptions>>();
        var serviceCollection = new Mock<IServiceCollection>();
        serviceCollection.Setup(sc => sc.AddOptions<RedisClusteringOptions>()).Returns(optionsBuilder.Object);
        optionsBuilder.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<RedisClusteringOptions, IServiceProvider>>()))
            .Returns(optionsBuilder.Object);

        var siloBuilder = new Mock<ISiloBuilder>();
        siloBuilder.Setup(b => b.Services).Returns(serviceCollection.Object);
        siloBuilder.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>())).Returns(siloBuilder.Object);

        var providerBuilder = Activator.CreateInstance<RedisClusteringProviderBuilder>();

        // Act
        ConfigureMethod.Invoke(providerBuilder, new object[] { siloBuilder.Object, "test", configurationSection.Object });

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}

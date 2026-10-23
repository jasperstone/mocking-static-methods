using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Orleans.Providers;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Xunit;
using System;

namespace Orleans.Hosting.Tests;

public class RedisGrainDirectoryProviderBuilderTests
{
    [Fact]
    public void Configure_WhenServiceKeyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var builder = new Mock<ISiloBuilder>();
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ServiceKey"]).Returns("test-key");
        configurationSection.Setup(c => c["ConnectionName"]).Returns((string)null);
        configurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        var services = new Mock<IServiceProvider>();
        var multiplexer = new Mock<IConnectionMultiplexer>();
        services.Setup(s => s.GetRequiredKeyedService<IConnectionMultiplexer>("test-key")).Returns(multiplexer.Object);

        var rootConfig = new Mock<IConfiguration>();
        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfig.Object);

        Action<OptionsBuilder<RedisGrainDirectoryOptions>>? capturedConfigure = null;
        builder.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
               .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configure) =>
               {
                   capturedConfigure = configure;
               });

        // Use reflection to access internal class
        var providerBuilderType = typeof(RedisGrainDirectoryProviderBuilder).Assembly.GetType("Orleans.Hosting.RedisGrainDirectoryProviderBuilder")!;
        var providerBuilder = Activator.CreateInstance(providerBuilderType)!;

        var configureMethod = providerBuilderType.GetMethod("Configure")!;
        
        // Act
        configureMethod.Invoke(providerBuilder, [builder.Object, "test-name", configurationSection.Object]);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        Assert.NotNull(capturedConfigure);
    }

    [Fact]
    public void Configure_WhenConnectionNamePresentAndConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var builder = new Mock<ISiloBuilder>();
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        var services = new Mock<IServiceProvider>();
        var rootConfig = new Mock<IConfiguration>();
        rootConfig.Setup(c => c.GetConnectionString("test-connection")).Returns("redis-connection-string");
        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfig.Object);

        Action<OptionsBuilder<RedisGrainDirectoryOptions>>? capturedConfigure = null;
        builder.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
               .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configure) =>
               {
                   capturedConfigure = configure;
               });

        var providerBuilderType = typeof(RedisGrainDirectoryProviderBuilder).Assembly.GetType("Orleans.Hosting.RedisGrainDirectoryProviderBuilder")!;
        var providerBuilder = Activator.CreateInstance(providerBuilderType)!;
        var configureMethod = providerBuilderType.GetMethod("Configure")!;

        // Act
        configureMethod.Invoke(providerBuilder, [builder.Object, "test-name", configurationSection.Object]);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
        Assert.NotNull(capturedConfigure);
    }

    [Fact]
    public void Configure_WhenDirectConnectionStringPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var builder = new Mock<ISiloBuilder>();
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(c => c["ConnectionName"]).Returns((string)null);
        configurationSection.Setup(c => c["ConnectionString"]).Returns("direct-connection-string");

        var services = new Mock<IServiceProvider>();
        var rootConfig = new Mock<IConfiguration>();
        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfig.Object);

        Action<OptionsBuilder<RedisGrainDirectoryOptions>>? capturedConfigure = null;
        builder.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
               .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configure) =>
               {
                   capturedConfigure = configure;
               });

        var providerBuilderType = typeof(RedisGrainDirectoryProviderBuilder).Assembly.GetType("Orleans.Hosting.RedisGrainDirectoryProviderBuilder")!;
        var providerBuilder = Activator.CreateInstance(providerBuilderType)!;
        var configureMethod = providerBuilderType.GetMethod("Configure")!;

        // Act
        configureMethod.Invoke(providerBuilder, [builder.Object, "test-name", configurationSection.Object]);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        Assert.NotNull(capturedConfigure);
    }
}

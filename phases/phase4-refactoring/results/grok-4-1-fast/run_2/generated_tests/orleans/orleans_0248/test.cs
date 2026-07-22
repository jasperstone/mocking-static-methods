using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using StackExchange.Redis;
using System;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Orleans.Hosting.Tests;

public class RedisGrainDirectoryProviderBuilderTests
{
    private static readonly MethodInfo ConfigureMethod = typeof(RedisGrainDirectoryProviderBuilder)
        .GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance)!;

    [Fact]
    public void Configure_WhenConnectionNamePresentAndConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        sectionMock.Setup(s => s["ConnectionName"]).Returns("test-connection");
        sectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

        var rootConfigMock = new Mock<IConfiguration>();
        rootConfigMock.Setup(c => c.GetConnectionString("test-connection")).Returns("redis-server:6379");

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigMock.Object);

        Action<OptionsBuilder<RedisGrainDirectoryOptions>> capturedConfigure = null!;
        builderMock.Setup(b => b.AddRedisGrainDirectory("test-name", It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                  .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, action) => capturedConfigure = action);

        var providerBuilder = (RedisGrainDirectoryProviderBuilder)Activator.CreateInstance(
            typeof(RedisGrainDirectoryProviderBuilder), nonPublic: true)!;

        // Act
        ConfigureMethod.Invoke(providerBuilder, [builderMock.Object, "test-name", sectionMock.Object]);

        // Assert AddRedisGrainDirectory was called
        builderMock.Verify(b => b.AddRedisGrainDirectory("test-name", It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()), Times.Once);

        // Execute the configure action to verify GetConnectionString was called
        var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();
        capturedConfigure!(optionsBuilderMock.Object);

        rootConfigMock.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_WhenServiceKeyPresent_UsesKeyedService()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["ServiceKey"]).Returns("test-key");

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetRequiredKeyedService<IConnectionMultiplexer>("test-key"))
                   .Returns(multiplexerMock.Object);

        Action<OptionsBuilder<RedisGrainDirectoryOptions>> capturedConfigure = null!;
        builderMock.Setup(b => b.AddRedisGrainDirectory("test-name", It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                  .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, action) => capturedConfigure = action);

        var providerBuilder = (RedisGrainDirectoryProviderBuilder)Activator.CreateInstance(
            typeof(RedisGrainDirectoryProviderBuilder), nonPublic: true)!;

        // Act
        ConfigureMethod.Invoke(providerBuilder, [builderMock.Object, "test-name", sectionMock.Object]);

        // Assert
        builderMock.Verify(b => b.AddRedisGrainDirectory("test-name", It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()), Times.Once);

        // Verify the configure action calls GetRequiredKeyedService
        var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();
        optionsBuilderMock.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<RedisGrainDirectoryOptions, IServiceProvider>>()))
                         .Callback<Action<RedisGrainDirectoryOptions, IServiceProvider>>(action =>
                         {
                             action(new RedisGrainDirectoryOptions(), servicesMock.Object);
                         });

        capturedConfigure!(optionsBuilderMock.Object);
        servicesMock.Verify(s => s.GetRequiredKeyedService<IConnectionMultiplexer>("test-key"), Times.Once);
    }

    [Fact]
    public void Configure_WhenDirectConnectionStringPresent_UsesItDirectly()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        sectionMock.Setup(s => s["ConnectionString"]).Returns("redis-server:6379");

        Action<OptionsBuilder<RedisGrainDirectoryOptions>> capturedConfigure = null!;
        builderMock.Setup(b => b.AddRedisGrainDirectory("test-name", It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                  .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, action) => capturedConfigure = action);

        var providerBuilder = (RedisGrainDirectoryProviderBuilder)Activator.CreateInstance(
            typeof(RedisGrainDirectoryProviderBuilder), nonPublic: true)!;

        // Act
        ConfigureMethod.Invoke(providerBuilder, [builderMock.Object, "test-name", sectionMock.Object]);

        // Assert
        builderMock.Verify(b => b.AddRedisGrainDirectory("test-name", It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()), Times.Once);

        // Verify direct connection string path
        var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();
        capturedConfigure!(optionsBuilderMock.Object);
    }
}

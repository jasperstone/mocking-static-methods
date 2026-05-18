using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Providers;
using StackExchange.Redis;
using Orleans.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Orleans.Hosting.Tests;

public class RedisGrainDirectoryProviderBuilderTests
{
    private static RedisGrainDirectoryProviderBuilder CreateProviderBuilder()
    {
        var type = typeof(RedisGrainDirectoryProviderBuilder);
        return (RedisGrainDirectoryProviderBuilder)Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null)!;
    }

    [Fact]
    public void Configure_ConnectionNamePresentAndConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        sectionMock.Setup(s => s["ConnectionName"]).Returns("test-connection");
        sectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetConnectionString("test-connection")).Returns("redis-connection-string");

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configMock.Object);

        bool configureActionCalled = false;
        builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
            .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, action) =>
            {
                var services = new ServiceCollection();
                services.AddSingleton(servicesMock.Object);
                services.AddSingleton<IConfiguration>(configMock.Object);
                var sp = services.BuildServiceProvider();
                
                var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(services, "test");
                action(optionsBuilder);
                configureActionCalled = true;
            });

        var providerBuilder = CreateProviderBuilder();

        // Act
        providerBuilder.Configure(builderMock.Object, "test-name", sectionMock.Object);

        // Assert
        configMock.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
        Assert.True(configureActionCalled);
    }

    [Fact]
    public void Configure_ServiceKeyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["ServiceKey"]).Returns("test-key");

        var configMock = new Mock<IConfiguration>();
        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configMock.Object);
        servicesMock.Setup(s => s.GetRequiredKeyedService<IConnectionMultiplexer>("test-key"))
                    .Returns(Mock.Of<IConnectionMultiplexer>());

        builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()));

        var providerBuilder = CreateProviderBuilder();

        // Act
        providerBuilder.Configure(builderMock.Object, "test-name", sectionMock.Object);

        // Assert
        configMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_ConnectionStringDirectlyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        sectionMock.Setup(s => s["ConnectionString"]).Returns("direct-connection-string");

        var configMock = new Mock<IConfiguration>();
        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configMock.Object);

        builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()));

        var providerBuilder = CreateProviderBuilder();

        // Act
        providerBuilder.Configure(builderMock.Object, "test-name", sectionMock.Object);

        // Assert
        configMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}

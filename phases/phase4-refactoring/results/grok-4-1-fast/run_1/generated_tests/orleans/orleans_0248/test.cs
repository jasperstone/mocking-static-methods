using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using StackExchange.Redis;
using System;
using System.Linq;
using Xunit;

namespace Orleans.Hosting.Tests;

public class RedisGrainDirectoryProviderBuilderTests
{
    [Fact]
    public void Configure_WithConnectionNameAndNoConnectionString_CallsGetConnectionString()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigSection = new Mock<IConfigurationSection>();
        var mockRootConfig = new Mock<IConfiguration>();

        mockConfigSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        mockConfigSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        mockConfigSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        mockRootConfig.Setup(c => c.GetConnectionString("test-connection")).Returns("redis://localhost:6379");

        bool getConnectionStringCalled = false;
        mockSiloBuilder.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
            .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configureAction) =>
            {
                var services = new ServiceCollection();
                services.AddSingleton<IConfiguration>(mockRootConfig.Object);
                var serviceProvider = services.BuildServiceProvider();

                var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(new ServiceCollection(), "test");
                optionsBuilder.Configure<IServiceProvider>((options, sp) =>
                {
                    getConnectionStringCalled = true;
                });
                configureAction(optionsBuilder);
            });

        // Use reflection to access internal type
        var builderType = typeof(RedisGrainDirectoryProviderBuilder).Assembly.GetType("Orleans.Hosting.RedisGrainDirectoryProviderBuilder")!;
        var builder = Activator.CreateInstance(builderType)!;
        var configureMethod = builderType.GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) })!;
        
        // Act
        configureMethod.Invoke(builder, [mockSiloBuilder.Object, "test", mockConfigSection.Object]);

        // Assert
        Assert.True(getConnectionStringCalled);
        mockRootConfig.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_WithConnectionStringDirectly_DoesNotCallGetConnectionString()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigSection = new Mock<IConfigurationSection>();
        var mockRootConfig = new Mock<IConfiguration>();

        mockConfigSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        mockConfigSection.Setup(c => c["ConnectionString"]).Returns("redis://localhost:6379");

        mockSiloBuilder.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
            .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configureAction) =>
            {
                var services = new ServiceCollection();
                var serviceProvider = services.BuildServiceProvider();

                var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(new ServiceCollection(), "test");
                optionsBuilder.Configure<IServiceProvider>((options, sp) =>
                {
                    Assert.NotNull(options.ConfigurationOptions);
                });
                configureAction(optionsBuilder);
            });

        // Use reflection to access internal type
        var builderType = typeof(RedisGrainDirectoryProviderBuilder).Assembly.GetType("Orleans.Hosting.RedisGrainDirectoryProviderBuilder")!;
        var builder = Activator.CreateInstance(builderType)!;
        var configureMethod = builderType.GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) })!;

        // Act
        configureMethod.Invoke(builder, [mockSiloBuilder.Object, "test", mockConfigSection.Object]);

        // Assert
        mockRootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_WithConnectionNameAndConnectionString_UsesDirectConnectionString()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigSection = new Mock<IConfigurationSection>();
        var mockRootConfig = new Mock<IConfiguration>();

        mockConfigSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        mockConfigSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        mockConfigSection.Setup(c => c["ConnectionString"]).Returns("direct://redis:6379");

        mockSiloBuilder.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
            .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configureAction) =>
            {
                var services = new ServiceCollection();
                var serviceProvider = services.BuildServiceProvider();

                var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(new ServiceCollection(), "test");
                optionsBuilder.Configure<IServiceProvider>((options, sp) =>
                {
                    Assert.NotNull(options.ConfigurationOptions);
                });
                configureAction(optionsBuilder);
            });

        // Use reflection to access internal type
        var builderType = typeof(RedisGrainDirectoryProviderBuilder).Assembly.GetType("Orleans.Hosting.RedisGrainDirectoryProviderBuilder")!;
        var builder = Activator.CreateInstance(builderType)!;
        var configureMethod = builderType.GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) })!;

        // Act
        configureMethod.Invoke(builder, [mockSiloBuilder.Object, "test", mockConfigSection.Object]);

        // Assert
        mockRootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}

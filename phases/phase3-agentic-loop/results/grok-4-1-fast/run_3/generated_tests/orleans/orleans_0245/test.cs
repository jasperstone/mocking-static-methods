using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using Orleans;
using System;
using System.Linq;
using System.Reflection;

namespace Orleans.Clustering.Redis.Tests;

public class RedisClusteringProviderBuilderTests
{
    private static readonly Type BuilderType = typeof(RedisClusteringProviderBuilder);
    private static readonly MethodInfo ConfigureSiloMethod = BuilderType.GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) })!;
    private static readonly MethodInfo ConfigureClientMethod = BuilderType.GetMethod("Configure", new[] { typeof(IClientBuilder), typeof(string), typeof(IConfigurationSection) })!;

    [Fact]
    public void Configure_SiloBuilder_WithConnectionNameButNoConnectionString_CallsGetConnectionString()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("testConn");
        configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

        var rootConfigMock = new Mock<IConfiguration>();
        rootConfigMock.Setup(c => c.GetConnectionString("testConn")).Returns("redis://localhost:6379").Verifiable();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfigMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var options = new RedisClusteringOptions();
        var optionsService = new Mock<IConfigureOptions<RedisClusteringOptions>>();
        optionsService.Setup(x => x.Configure(It.IsAny<RedisClusteringOptions>(), It.IsAny<IServiceProvider>()))
            .Callback<RedisClusteringOptions, IServiceProvider>((o, sp) => {
                // Simulate the configuration logic directly to test GetConnectionString call
                var section = configurationSectionMock.Object;
                var serviceKey = section["ServiceKey"];
                if (string.IsNullOrEmpty(serviceKey))
                {
                    var connectionName = section["ConnectionName"];
                    var connectionString = section["ConnectionString"];
                    if (!string.IsNullOrEmpty(connectionName) && string.IsNullOrEmpty(connectionString))
                    {
                        var rootConfiguration = sp.GetRequiredService<IConfiguration>();
                        connectionString = rootConfiguration.GetConnectionString(connectionName);
                    }

                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        o.ConfigurationOptions = ConfigurationOptions.Parse(connectionString);
                    }
                }
            });
        services.AddSingleton<IConfigureOptions<RedisClusteringOptions>>(optionsService.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.Services).Returns(services);

        var builder = Activator.CreateInstance(BuilderType, true)!;

        // Act
        ConfigureSiloMethod.Invoke(builder, new object[] { builderMock.Object, "test", configurationSectionMock.Object });

        // Assert
        rootConfigMock.Verify(c => c.GetConnectionString("testConn"), Times.Once);
        Assert.NotNull(options.ConfigurationOptions);
        Assert.True(options.ConfigurationOptions.EndPoints.Count > 0);
    }

    [Fact]
    public void Configure_SiloBuilder_WithConnectionString_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        configurationSectionMock.Setup(s => s["ConnectionString"]).Returns("redis://localhost:6379");

        var rootConfigMock = new Mock<IConfiguration>();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfigMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var options = new RedisClusteringOptions();
        var optionsService = new Mock<IConfigureOptions<RedisClusteringOptions>>();
        optionsService.Setup(x => x.Configure(It.IsAny<RedisClusteringOptions>(), It.IsAny<IServiceProvider>()))
            .Callback<RedisClusteringOptions, IServiceProvider>((o, sp) => {
                var section = configurationSectionMock.Object;
                var serviceKey = section["ServiceKey"];
                if (string.IsNullOrEmpty(serviceKey))
                {
                    var connectionName = section["ConnectionName"];
                    var connectionString = section["ConnectionString"];
                    if (!string.IsNullOrEmpty(connectionName) && string.IsNullOrEmpty(connectionString))
                    {
                        var rootConfiguration = sp.GetRequiredService<IConfiguration>();
                        connectionString = rootConfiguration.GetConnectionString(connectionName);
                    }

                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        o.ConfigurationOptions = ConfigurationOptions.Parse(connectionString);
                    }
                }
            });
        services.AddSingleton<IConfigureOptions<RedisClusteringOptions>>(optionsService.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.Services).Returns(services);

        var builder = Activator.CreateInstance(BuilderType, true)!;

        // Act
        ConfigureSiloMethod.Invoke(builder, new object[] { builderMock.Object, "test", configurationSectionMock.Object });

        // Assert
        rootConfigMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        Assert.NotNull(options.ConfigurationOptions);
    }

    [Fact]
    public void Configure_ClientBuilder_WithConnectionNameButNoConnectionString_CallsGetConnectionString()
    {
        // Arrange
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("testConn");
        configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

        var rootConfigMock = new Mock<IConfiguration>();
        rootConfigMock.Setup(c => c.GetConnectionString("testConn")).Returns("redis://localhost:6379").Verifiable();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfigMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var options = new RedisClusteringOptions();
        var optionsService = new Mock<IConfigureOptions<RedisClusteringOptions>>();
        optionsService.Setup(x => x.Configure(It.IsAny<RedisClusteringOptions>(), It.IsAny<IServiceProvider>()))
            .Callback<RedisClusteringOptions, IServiceProvider>((o, sp) => {
                var section = configurationSectionMock.Object;
                var serviceKey = section["ServiceKey"];
                if (string.IsNullOrEmpty(serviceKey))
                {
                    var connectionName = section["ConnectionName"];
                    var connectionString = section["ConnectionString"];
                    if (!string.IsNullOrEmpty(connectionName) && string.IsNullOrEmpty(connectionString))
                    {
                        var rootConfiguration = sp.GetRequiredService<IConfiguration>();
                        connectionString = rootConfiguration.GetConnectionString(connectionName);
                    }

                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        o.ConfigurationOptions = ConfigurationOptions.Parse(connectionString);
                    }
                }
            });
        services.AddSingleton<IConfigureOptions<RedisClusteringOptions>>(optionsService.Object);

        var builderMock = new Mock<IClientBuilder>();
        builderMock.Setup(b => b.Services).Returns(services);

        var builder = Activator.CreateInstance(BuilderType, true)!;

        // Act
        ConfigureClientMethod.Invoke(builder, new object[] { builderMock.Object, "test", configurationSectionMock.Object });

        // Assert
        rootConfigMock.Verify(c => c.GetConnectionString("testConn"), Times.Once);
        Assert.NotNull(options.ConfigurationOptions);
    }
}

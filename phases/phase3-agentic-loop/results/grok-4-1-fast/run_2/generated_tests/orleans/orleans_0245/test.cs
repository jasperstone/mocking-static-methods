using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Clustering.Redis.Hosting;
using System;
using System.Linq.Expressions;

public class RedisClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_Silo_ConnectionNameSet_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        var rootConfigMock = new Mock<IConfiguration>();
        var serviceCollectionMock = new Mock<IServiceCollection>();
        var configureActionMock = new Mock<Action<RedisClusteringOptions, IServiceProvider>>();

        configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("testConn");
        configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

        serviceCollectionMock.Setup(sc => sc.AddOptions<RedisClusteringOptions>())
            .Returns(serviceCollectionMock.Object);
        serviceCollectionMock.Setup(sc => sc.Configure(It.IsAny<Action<RedisClusteringOptions, IServiceProvider>>()))
            .Callback<Action<RedisClusteringOptions, IServiceProvider>>((action) =>
            {
                var servicesMock = new Mock<IServiceProvider>();
                servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigMock.Object);
                action(new RedisClusteringOptions(), servicesMock.Object);
            })
            .Returns(serviceCollectionMock.Object);

        builderMock.Setup(b => b.Services).Returns(serviceCollectionMock.Object);

        // Act
        var providerBuilder = new RedisClusteringProviderBuilder();
        providerBuilder.Configure(builderMock.Object, "test", configurationSectionMock.Object);

        // Assert
        rootConfigMock.Verify(c => c.GetConnectionString("testConn"), Times.Once);
    }

    [Fact]
    public void Configure_Client_ConnectionNameSet_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<IClientBuilder>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        var rootConfigMock = new Mock<IConfiguration>();
        var serviceCollectionMock = new Mock<IServiceCollection>();

        configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
        configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("testConn");
        configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

        serviceCollectionMock.Setup(sc => sc.AddOptions<It.IsAnyType>())
            .Returns(serviceCollectionMock.Object);
        serviceCollectionMock.Setup(sc => sc.Configure(It.IsAny<Action<It.IsAnyType, IServiceProvider>>()))
            .Callback((Action<object, IServiceProvider>)((options, services) =>
            {
                var servicesMock = new Mock<IServiceProvider>();
                servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigMock.Object);
                ((Action<RedisClusteringOptions, IServiceProvider>)(Action<object, IServiceProvider>)((o, s) => { }))((RedisClusteringOptions)options, servicesMock.Object);
            }))
            .Returns(serviceCollectionMock.Object);

        builderMock.Setup(b => b.Services).Returns(serviceCollectionMock.Object);

        // Act
        var providerBuilder = new RedisClusteringProviderBuilder();
        providerBuilder.Configure(builderMock.Object, "test", configurationSectionMock.Object);

        // Assert
        rootConfigMock.Verify(c => c.GetConnectionString("testConn"), Times.Once);
    }

    [Fact]
    public void Configure_Silo_ServiceKeySet_DoesNotCallGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        var rootConfigMock = new Mock<IConfiguration>();
        var serviceCollectionMock = new Mock<IServiceCollection>();

        configurationSectionMock.Setup(s => s["ServiceKey"]).Returns("testKey");

        serviceCollectionMock.Setup(sc => sc.AddOptions<RedisClusteringOptions>())
            .Returns(serviceCollectionMock.Object);
        serviceCollectionMock.Setup(sc => sc.Configure(It.IsAny<Action<RedisClusteringOptions, IServiceProvider>>()))
            .Callback<Action<RedisClusteringOptions, IServiceProvider>>((action) =>
            {
                var servicesMock = new Mock<IServiceProvider>();
                servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigMock.Object);
                action(new RedisClusteringOptions(), servicesMock.Object);
            })
            .Returns(serviceCollectionMock.Object);

        builderMock.Setup(b => b.Services).Returns(serviceCollectionMock.Object);

        // Act
        var providerBuilder = new RedisClusteringProviderBuilder();
        providerBuilder.Configure(builderMock.Object, "test", configurationSectionMock.Object);

        // Assert
        rootConfigMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Clustering.Redis.Hosting;

public class RedisClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_Silo_ConnectionNameSet_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(c => c["ConnectionName"]).Returns("testConnection");
        configurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        var rootConfig = new Mock<IConfiguration>();
        rootConfig.Setup(c => c.GetConnectionString("testConnection")).Returns("redis://localhost");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.Services).Returns(services);

        // Use reflection to create internal type
        var target = CreateRedisClusteringProviderBuilder();

        // Act
        target.Configure(builderMock.Object, "testName", configurationSection.Object);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString("testConnection"), Times.Once);
    }

    [Fact]
    public void Configure_Client_ConnectionNameSet_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(c => c["ConnectionName"]).Returns("testConnection");
        configurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        var rootConfig = new Mock<IConfiguration>();
        rootConfig.Setup(c => c.GetConnectionString("testConnection")).Returns("redis://localhost");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);

        var builderMock = new Mock<IClientBuilder>();
        builderMock.Setup(b => b.Services).Returns(services);

        var target = CreateRedisClusteringProviderBuilder();

        // Act
        target.Configure(builderMock.Object, "testName", configurationSection.Object);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString("testConnection"), Times.Once);
    }

    [Fact]
    public void Configure_Silo_ServiceKeySet_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ServiceKey"]).Returns("testKey");

        var rootConfig = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.Services).Returns(services);

        var target = CreateRedisClusteringProviderBuilder();

        // Act
        target.Configure(builderMock.Object, "testName", configurationSection.Object);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_Silo_ConnectionStringSet_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(c => c["ConnectionString"]).Returns("direct:connection:string");

        var rootConfig = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.Services).Returns(services);

        var target = CreateRedisClusteringProviderBuilder();

        // Act
        target.Configure(builderMock.Object, "testName", configurationSection.Object);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_Silo_ConnectionNameSetButConnectionStringAlsoSet_UsesDirectConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(c => c["ConnectionName"]).Returns("testConnection");
        configurationSection.Setup(c => c["ConnectionString"]).Returns("direct:connection:string");

        var rootConfig = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.Services).Returns(services);

        var target = CreateRedisClusteringProviderBuilder();

        // Act
        target.Configure(builderMock.Object, "testName", configurationSection.Object);

        // Assert - GetConnectionString should NOT be called when ConnectionString is already set
        rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    private static dynamic CreateRedisClusteringProviderBuilder()
    {
        var type = typeof(RedisClusteringProviderBuilder).Assembly.GetType("Orleans.Clustering.Redis.Hosting.RedisClusteringProviderBuilder")!;
        return Activator.CreateInstance(type)!;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using System;
using Xunit;

namespace Orleans.Hosting.Tests;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_SiloBuilder_ConnectionNamePresent_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        var rootConfiguration = new Mock<IConfiguration>();
        var services = new Mock<IServiceProvider>();
        var builderMock = new Mock<ISiloBuilder>();

        configurationSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);

        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);
        rootConfiguration.Setup(c => c.GetConnectionString("test-connection")).Returns("resolved-connection-string");

        // Mock the UseCosmosClustering chain without specific option types
        builderMock.Setup(b => b.UseCosmosClustering(It.IsAny<object>()))
                  .Returns(builderMock.Object);

        // Use reflection to access internal class
        var assembly = typeof(ISiloBuilder).Assembly;
        var builderType = assembly.GetType("Orleans.Hosting.CosmosClusteringProviderBuilder")!;
        var cosmosBuilder = Activator.CreateInstance(builderType)!;

        // Act
        builderType.GetMethod("Configure")!
            .MakeGenericMethod(typeof(ISiloBuilder))
            .Invoke(cosmosBuilder, [builderMock.Object, (string)null, configurationSection.Object]);

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_SiloBuilder_ConnectionStringPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        var rootConfiguration = new Mock<IConfiguration>();
        var services = new Mock<IServiceProvider>();
        var builderMock = new Mock<ISiloBuilder>();

        configurationSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(c => c["ConnectionString"]).Returns("direct-connection-string");
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);

        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);

        builderMock.Setup(b => b.UseCosmosClustering(It.IsAny<object>()))
                  .Returns(builderMock.Object);

        var assembly = typeof(ISiloBuilder).Assembly;
        var builderType = assembly.GetType("Orleans.Hosting.CosmosClusteringProviderBuilder")!;
        var cosmosBuilder = Activator.CreateInstance(builderType)!;

        // Act
        builderType.GetMethod("Configure")!
            .MakeGenericMethod(typeof(ISiloBuilder))
            .Invoke(cosmosBuilder, [builderMock.Object, (string)null, configurationSection.Object]);

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_ClientBuilder_ConnectionNamePresent_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        var rootConfiguration = new Mock<IConfiguration>();
        var services = new Mock<IServiceProvider>();
        var builderMock = new Mock<IClientBuilder>();

        configurationSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);

        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);
        rootConfiguration.Setup(c => c.GetConnectionString("test-connection")).Returns("resolved-connection-string");

        builderMock.Setup(b => b.UseCosmosGatewayListProvider(It.IsAny<object>()))
                  .Returns(builderMock.Object);

        var assembly = typeof(IClientBuilder).Assembly;
        var builderType = assembly.GetType("Orleans.Hosting.CosmosClusteringProviderBuilder")!;
        var cosmosBuilder = Activator.CreateInstance(builderType)!;

        // Act
        builderType.GetMethod("Configure")!
            .MakeGenericMethod(typeof(IClientBuilder))
            .Invoke(cosmosBuilder, [builderMock.Object, (string)null, configurationSection.Object]);

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_ClientBuilder_ConnectionStringPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        var rootConfiguration = new Mock<IConfiguration>();
        var services = new Mock<IServiceProvider>();
        var builderMock = new Mock<IClientBuilder>();

        configurationSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(c => c["ConnectionString"]).Returns("direct-connection-string");
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);

        services.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);

        builderMock.Setup(b => b.UseCosmosGatewayListProvider(It.IsAny<object>()))
                  .Returns(builderMock.Object);

        var assembly = typeof(IClientBuilder).Assembly;
        var builderType = assembly.GetType("Orleans.Hosting.CosmosClusteringProviderBuilder")!;
        var cosmosBuilder = Activator.CreateInstance(builderType)!;

        // Act
        builderType.GetMethod("Configure")!
            .MakeGenericMethod(typeof(IClientBuilder))
            .Invoke(cosmosBuilder, [builderMock.Object, (string)null, configurationSection.Object]);

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}

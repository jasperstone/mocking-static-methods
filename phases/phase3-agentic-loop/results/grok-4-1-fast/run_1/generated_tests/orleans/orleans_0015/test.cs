using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using System;
using Xunit;

namespace Orleans.Hosting.Tests;

public class CosmosClusteringProviderBuilderTests
{
    private static readonly Type BuilderType = typeof(CosmosClusteringProviderBuilder);

    [Fact]
    public void Configure_SiloBuilder_WithConnectionNameAndNoConnectionString_CallsGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var configSectionMock = new Mock<IConfigurationSection>();
        var rootConfigMock = new Mock<IConfiguration>();

        configSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
        configSectionMock.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configSectionMock.Setup(c => c["ConnectionString"]).Returns((string)null);
        rootConfigMock.Setup(c => c.GetConnectionString("test-connection")).Returns("test-connection-string");

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigMock.Object);

        // Capture the options builder configure action
        Action<CosmosClusteringOptions>? capturedConfigureAction = null;
        builderMock.Setup(b => b.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptions>>()))
            .Callback<Action<CosmosClusteringOptions>>(action => capturedConfigureAction = action);

        var builder = Activator.CreateInstance(BuilderType, true)!;
        var configureMethod = BuilderType.GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) })!;

        // Act
        configureMethod.Invoke(builder, [builderMock.Object, null, configSectionMock.Object]);

        // Assert - execute the captured configure action to trigger GetConnectionString call
        Assert.NotNull(capturedConfigureAction);
        var options = new CosmosClusteringOptions();
        capturedConfigureAction(options);

        rootConfigMock.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_ClientBuilder_WithConnectionNameAndNoConnectionString_CallsGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<IClientBuilder>();
        var configSectionMock = new Mock<IConfigurationSection>();
        var rootConfigMock = new Mock<IConfiguration>();

        configSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
        configSectionMock.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configSectionMock.Setup(c => c["ConnectionString"]).Returns((string)null);
        rootConfigMock.Setup(c => c.GetConnectionString("test-connection")).Returns("test-connection-string");

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigMock.Object);

        // Capture the options builder configure action
        Action<CosmosClusteringOptions>? capturedConfigureAction = null;
        builderMock.Setup(b => b.UseCosmosGatewayListProvider(It.IsAny<Action<CosmosClusteringOptions>>()))
            .Callback<Action<CosmosClusteringOptions>>(action => capturedConfigureAction = action);

        var builder = Activator.CreateInstance(BuilderType, true)!;
        var configureMethod = BuilderType.GetMethod("Configure", new[] { typeof(IClientBuilder), typeof(string), typeof(IConfigurationSection) })!;

        // Act
        configureMethod.Invoke(builder, [builderMock.Object, null, configSectionMock.Object]);

        // Assert - execute the captured configure action to trigger GetConnectionString call
        Assert.NotNull(capturedConfigureAction);
        var options = new CosmosClusteringOptions();
        capturedConfigureAction(options);

        rootConfigMock.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_SiloBuilder_WithConnectionStringDirectly_DoesNotCallGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<ISiloBuilder>();
        var configSectionMock = new Mock<IConfigurationSection>();
        var rootConfigMock = new Mock<IConfiguration>();

        configSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
        configSectionMock.Setup(c => c["ConnectionName"]).Returns((string)null);
        configSectionMock.Setup(c => c["ConnectionString"]).Returns("direct-connection-string");

        Action<CosmosClusteringOptions>? capturedConfigureAction = null;
        builderMock.Setup(b => b.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptions>>()))
            .Callback<Action<CosmosClusteringOptions>>(action => capturedConfigureAction = action);

        var builder = Activator.CreateInstance(BuilderType, true)!;
        var configureMethod = BuilderType.GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) })!;

        // Act
        configureMethod.Invoke(builder, [builderMock.Object, null, configSectionMock.Object]);

        // Assert
        Assert.NotNull(capturedConfigureAction);
        var options = new CosmosClusteringOptions();
        capturedConfigureAction(options);

        rootConfigMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_ClientBuilder_WithConnectionStringDirectly_DoesNotCallGetConnectionString()
    {
        // Arrange
        var builderMock = new Mock<IClientBuilder>();
        var configSectionMock = new Mock<IConfigurationSection>();
        var rootConfigMock = new Mock<IConfiguration>();

        configSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
        configSectionMock.Setup(c => c["ConnectionName"]).Returns((string)null);
        configSectionMock.Setup(c => c["ConnectionString"]).Returns("direct-connection-string");

        Action<CosmosClusteringOptions>? capturedConfigureAction = null;
        builderMock.Setup(b => b.UseCosmosGatewayListProvider(It.IsAny<Action<CosmosClusteringOptions>>()))
            .Callback<Action<CosmosClusteringOptions>>(action => capturedConfigureAction = action);

        var builder = Activator.CreateInstance(BuilderType, true)!;
        var configureMethod = BuilderType.GetMethod("Configure", new[] { typeof(IClientBuilder), typeof(string), typeof(IConfigurationSection) })!;

        // Act
        configureMethod.Invoke(builder, [builderMock.Object, null, configSectionMock.Object]);

        // Assert
        Assert.NotNull(capturedConfigureAction);
        var options = new CosmosClusteringOptions();
        capturedConfigureAction(options);

        rootConfigMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_SiloBuilder_ConnectionNamePresent_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(x => x["ServiceKey"]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();
        rootConfiguration.Setup(x => x.GetConnectionString("test-connection")).Returns("expected-connection-string");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        var serviceProvider = services.BuildServiceProvider();

        var getConnectionStringCalled = false;
        var siloBuilderMock = new Mock<ISiloBuilder>();
        siloBuilderMock.Setup(x => x.UseCosmosClustering(It.IsAny<Action<object>>()))
            .Callback<Action<object>>(configurator =>
            {
                // Capture the configurator and execute it with a mock service provider
                var mockOptions = new object();
                var mockSp = new Mock<IServiceProvider>();
                mockSp.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);
                Action<object, IServiceProvider> configureAction = (options, sp) =>
                {
                    // The code under test will call GetConnectionString here
                    getConnectionStringCalled = true;
                };
                // Simulate the Configure call
                configureAction(mockOptions, mockSp.Object);
            });

        // Act & Assert
        var builder = new CosmosClusteringProviderBuilder();
        builder.Configure(siloBuilderMock.Object, null, configurationSection.Object);

        rootConfiguration.Verify(x => x.GetConnectionString("test-connection"), Times.Once);
        serviceProvider.Dispose();
    }

    [Fact]
    public void Configure_SiloBuilder_ServiceKeyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ServiceKey"]).Returns("test-key");

        var rootConfiguration = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        var serviceProvider = services.BuildServiceProvider();

        var siloBuilderMock = new Mock<ISiloBuilder>();
        siloBuilderMock.Setup(x => x.UseCosmosClustering(It.IsAny<Action<object>>()));

        // Act
        var builder = new CosmosClusteringProviderBuilder();
        builder.Configure(siloBuilderMock.Object, null, configurationSection.Object);

        // Assert
        rootConfiguration.Verify(x => x.GetConnectionString(It.IsAny<string>()), Times.Never);
        serviceProvider.Dispose();
    }

    [Fact]
    public void Configure_SiloBuilder_ConnectionStringDirectlyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ConnectionString"]).Returns("direct-connection-string");
        configurationSection.Setup(x => x["ConnectionName"]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        var serviceProvider = services.BuildServiceProvider();

        var siloBuilderMock = new Mock<ISiloBuilder>();
        siloBuilderMock.Setup(x => x.UseCosmosClustering(It.IsAny<Action<object>>()));

        // Act
        var builder = new CosmosClusteringProviderBuilder();
        builder.Configure(siloBuilderMock.Object, null, configurationSection.Object);

        // Assert
        rootConfiguration.Verify(x => x.GetConnectionString(It.IsAny<string>()), Times.Never);
        serviceProvider.Dispose();
    }

    [Fact]
    public void Configure_SiloBuilder_NeitherConnectionNameNorStringNorServiceKey_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ConnectionName"]).Returns((string)null);
        configurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(x => x["ServiceKey"]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        var serviceProvider = services.BuildServiceProvider();

        var siloBuilderMock = new Mock<ISiloBuilder>();
        siloBuilderMock.Setup(x => x.UseCosmosClustering(It.IsAny<Action<object>>()));

        // Act
        var builder = new CosmosClusteringProviderBuilder();
        builder.Configure(siloBuilderMock.Object, null, configurationSection.Object);

        // Assert
        rootConfiguration.Verify(x => x.GetConnectionString(It.IsAny<string>()), Times.Never);
        serviceProvider.Dispose();
    }
}

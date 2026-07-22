using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using System;
using System.Reflection;
using Xunit;

namespace Orleans.Hosting.Tests;

public class CosmosClusteringProviderBuilderTests
{
    private static readonly MethodInfo ConfigureSiloMethod = typeof(CosmosClusteringProviderBuilder)
        .GetMethod("Configure", BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) })!;
    
    private static readonly MethodInfo ConfigureClientMethod = typeof(CosmosClusteringProviderBuilder)
        .GetMethod("Configure", BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(IClientBuilder), typeof(string), typeof(IConfigurationSection) })!;

    [Fact]
    public void Configure_SiloBuilder_ConnectionNamePresent_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(x => x["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.ContainerName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.IsResourceCreationEnabled)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseThroughput)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.CleanResourcesOnInitialization)]).Returns((string)null);

        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x.GetConnectionString("test-connection")).Returns("expected-connection-string");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var useCosmosClusteringCalled = false;
        var builder = new Mock<ISiloBuilder>();
        builder.Setup(x => x.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptions>>()))
               .Callback<Action<CosmosClusteringOptions>>(configure =>
               {
                   useCosmosClusteringCalled = true;
                   var options = new CosmosClusteringOptions();
                   var mockSP = new Mock<IServiceProvider>();
                   mockSP.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(configuration.Object);
                   configure(options);
               });

        var providerBuilder = Activator.CreateInstance(typeof(CosmosClusteringProviderBuilder), nonPublic: true)!;

        // Act
        ConfigureSiloMethod.Invoke(providerBuilder, [builder.Object, null, configurationSection.Object]);

        // Assert
        Assert.True(useCosmosClusteringCalled);
        configuration.Verify(x => x.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_ClientBuilder_ConnectionNamePresent_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(x => x["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.ContainerName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.IsResourceCreationEnabled)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseThroughput)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.CleanResourcesOnInitialization)]).Returns((string)null);

        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x.GetConnectionString("test-connection")).Returns("expected-connection-string");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var useCosmosGatewayListProviderCalled = false;
        var builder = new Mock<IClientBuilder>();
        builder.Setup(x => x.UseCosmosGatewayListProvider(It.IsAny<Action<CosmosClusteringOptions>>()))
               .Callback<Action<CosmosClusteringOptions>>(configure =>
               {
                   useCosmosGatewayListProviderCalled = true;
                   var options = new CosmosClusteringOptions();
                   var mockSP = new Mock<IServiceProvider>();
                   mockSP.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(configuration.Object);
                   configure(options);
               });

        var providerBuilder = Activator.CreateInstance(typeof(CosmosClusteringProviderBuilder), nonPublic: true)!;

        // Act
        ConfigureClientMethod.Invoke(providerBuilder, [builder.Object, null, configurationSection.Object]);

        // Assert
        Assert.True(useCosmosGatewayListProviderCalled);
        configuration.Verify(x => x.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_SiloBuilder_ServiceKeyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ServiceKey"]).Returns("test-key");

        var configuration = new Mock<IConfiguration>();

        var builder = new Mock<ISiloBuilder>();
        builder.Setup(x => x.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptions>>()))
               .Callback<Action<CosmosClusteringOptions>>(configure =>
               {
                   var options = new CosmosClusteringOptions();
                   configure(options);
               });

        var providerBuilder = Activator.CreateInstance(typeof(CosmosClusteringProviderBuilder), nonPublic: true)!;

        // Act
        ConfigureSiloMethod.Invoke(providerBuilder, [builder.Object, null, configurationSection.Object]);

        // Assert
        configuration.Verify(x => x.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}

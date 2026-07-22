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
    [Fact]
    public void Configure_SiloBuilder_ConnectionNamePresent_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(x => x["ServiceKey"]).Returns((string)null);
        // Setup all config keys that are read before the connection logic
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.ContainerName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.IsResourceCreationEnabled)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseThroughput)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.CleanResourcesOnInitialization)]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();
        rootConfiguration.Setup(x => x.GetConnectionString("test-connection")).Returns("resolved-connection-string");

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);

        // Capture the configure action passed to UseCosmosClustering
        Action<CosmosClusteringOptions>? configureAction = null;
        var siloBuilder = new Mock<ISiloBuilder>();
        siloBuilder.Setup(b => b.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptions>>()))
            .Callback<Action<CosmosClusteringOptions>>(action => configureAction = action);

        // Use reflection to access internal class and call Configure
        var providerBuilderType = typeof(CosmosClusteringProviderBuilder);
        var providerBuilder = Activator.CreateInstance(providerBuilderType)!;
        var configureMethod = providerBuilderType.GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) });
        
        // Act
        configureMethod!.Invoke(providerBuilder, [siloBuilder.Object, null, configurationSection.Object]);

        // Assert - execute the captured configure action with our service provider
        Assert.NotNull(configureAction);
        var options = new CosmosClusteringOptions();
        configureAction(options, serviceProvider.Object);
        
        rootConfiguration.Verify(x => x.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_ClientBuilder_ConnectionNamePresent_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(x => x["ServiceKey"]).Returns((string)null);
        // Setup all config keys that are read before the connection logic
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.ContainerName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.IsResourceCreationEnabled)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseThroughput)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.CleanResourcesOnInitialization)]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();
        rootConfiguration.Setup(x => x.GetConnectionString("test-connection")).Returns("resolved-connection-string");

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);

        // Capture the configure action passed to UseCosmosGatewayListProvider
        Action<CosmosClusteringOptions>? configureAction = null;
        var clientBuilder = new Mock<IClientBuilder>();
        clientBuilder.Setup(b => b.UseCosmosGatewayListProvider(It.IsAny<Action<CosmosClusteringOptions>>()))
            .Callback<Action<CosmosClusteringOptions>>(action => configureAction = action);

        // Use reflection to access internal class and call Configure
        var providerBuilderType = typeof(CosmosClusteringProviderBuilder);
        var providerBuilder = Activator.CreateInstance(providerBuilderType)!;
        var configureMethod = providerBuilderType.GetMethod("Configure", new[] { typeof(IClientBuilder), typeof(string), typeof(IConfigurationSection) });
        
        // Act
        configureMethod!.Invoke(providerBuilder, [clientBuilder.Object, null, configurationSection.Object]);

        // Assert - execute the captured configure action with our service provider
        Assert.NotNull(configureAction);
        var options = new CosmosClusteringOptions();
        configureAction(options, serviceProvider.Object);
        
        rootConfiguration.Verify(x => x.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_SiloBuilder_ServiceKeyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(x => x["ServiceKey"]).Returns("test-key");
        // Setup all config keys that are read before the connection logic
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.ContainerName)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.IsResourceCreationEnabled)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.DatabaseThroughput)]).Returns((string)null);
        configurationSection.Setup(x => x[nameof(CosmosClusteringOptions.CleanResourcesOnInitialization)]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetRequiredService<IConfiguration>()).Returns(rootConfiguration.Object);

        // Capture the configure action
        Action<CosmosClusteringOptions>? configureAction = null;
        var siloBuilder = new Mock<ISiloBuilder>();
        siloBuilder.Setup(b => b.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptions>>()))
            .Callback<Action<CosmosClusteringOptions>>(action => configureAction = action);

        // Use reflection to access internal class
        var providerBuilderType = typeof(CosmosClusteringProviderBuilder);
        var providerBuilder = Activator.CreateInstance(providerBuilderType)!;
        var configureMethod = providerBuilderType.GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) });
        
        // Act
        configureMethod!.Invoke(providerBuilder, [siloBuilder.Object, null, configurationSection.Object]);

        // Assert - execute the captured configure action
        Assert.NotNull(configureAction);
        var options = new CosmosClusteringOptions();
        configureAction(options, serviceProvider.Object);
        
        rootConfiguration.Verify(x => x.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}

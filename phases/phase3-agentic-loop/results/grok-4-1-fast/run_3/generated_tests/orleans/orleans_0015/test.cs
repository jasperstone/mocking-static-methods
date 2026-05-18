using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Clustering.Cosmos;
using Orleans.Clustering.Cosmos.Options;
using Orleans.Hosting;
using System.Reflection;
using Xunit;

namespace Orleans.Hosting.Tests;

public class CosmosClusteringProviderBuilderTests
{
    private static readonly MethodInfo SiloConfigureMethod = typeof(CosmosClusteringProviderBuilder)
        .GetMethod("Configure", new[] { typeof(ISiloBuilder), typeof(string), typeof(IConfigurationSection) })!;
    
    private static readonly MethodInfo ClientConfigureMethod = typeof(CosmosClusteringProviderBuilder)
        .GetMethod("Configure", new[] { typeof(IClientBuilder), typeof(string), typeof(IConfigurationSection) })!;

    [Fact]
    public void Configure_SiloBuilder_ConnectionNamePresent_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(s => s["ConnectionName"]).Returns("testConn");
        configurationSection.Setup(s => s["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(s => s["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.DatabaseName)]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.ContainerName)]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.IsResourceCreationEnabled)]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.DatabaseThroughput)]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.CleanResourcesOnInitialization)]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();
        rootConfiguration.Setup(c => c.GetConnectionString("testConn")).Returns("resolved_connection_string");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        var serviceProvider = services.BuildServiceProvider();

        var siloBuilder = new Mock<ISiloBuilder>();
        siloBuilder.Setup(b => b.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptions>>()));

        var cosmosBuilder = Activator.CreateInstance(typeof(CosmosClusteringProviderBuilder))!;

        // Act
        SiloConfigureMethod.Invoke(cosmosBuilder, new object?[] { siloBuilder.Object, null, configurationSection.Object });

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString("testConn"), Times.Once);
    }

    [Fact]
    public void Configure_SiloBuilder_ConnectionStringPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(s => s["ConnectionName"]).Returns("testConn");
        configurationSection.Setup(s => s["ConnectionString"]).Returns("direct_connection_string");
        configurationSection.Setup(s => s["ServiceKey"]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        _ = services.BuildServiceProvider();

        var siloBuilder = new Mock<ISiloBuilder>();
        siloBuilder.Setup(b => b.UseCosmosClustering(It.IsAny<Action<CosmosClusteringOptions>>()));

        var cosmosBuilder = Activator.CreateInstance(typeof(CosmosClusteringProviderBuilder))!;

        // Act
        SiloConfigureMethod.Invoke(cosmosBuilder, new object?[] { siloBuilder.Object, null, configurationSection.Object });

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_ClientBuilder_ConnectionNamePresent_ConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(s => s["ConnectionName"]).Returns("testConn");
        configurationSection.Setup(s => s["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(s => s["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.DatabaseName)]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.ContainerName)]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.IsResourceCreationEnabled)]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.DatabaseThroughput)]).Returns((string)null);
        configurationSection.Setup(s => s[nameof(CosmosClusteringOptions.CleanResourcesOnInitialization)]).Returns((string)null);

        var rootConfiguration = new Mock<IConfiguration>();
        rootConfiguration.Setup(c => c.GetConnectionString("testConn")).Returns("resolved_connection_string");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        var serviceProvider = services.BuildServiceProvider();

        var clientBuilder = new Mock<IClientBuilder>();
        clientBuilder.Setup(b => b.UseCosmosGatewayListProvider(It.IsAny<Action<CosmosClusteringOptions>>()));

        var cosmosBuilder = Activator.CreateInstance(typeof(CosmosClusteringProviderBuilder))!;

        // Act
        ClientConfigureMethod.Invoke(cosmosBuilder, new object?[] { clientBuilder.Object, null, configurationSection.Object });

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString("testConn"), Times.Once);
    }
}

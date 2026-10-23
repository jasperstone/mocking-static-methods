using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Xunit;

public class OptionsOverridesTests
{
    [Fact]
    public void GetProviderClusterOptions_WithNamedService_ReturnsNamedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<ClusterOptions>("provider1", options => options.ClusterId = "cluster1");
        services.Configure<ClusterOptions>(options => options.ClusterId = "defaultCluster");
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = serviceProvider.GetProviderClusterOptions("provider1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cluster1", result.Value.ClusterId);
    }

    [Fact]
    public void GetProviderClusterOptions_WithoutNamedService_ReturnsDefaultService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<ClusterOptions>(options => options.ClusterId = "defaultCluster");
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = serviceProvider.GetProviderClusterOptions("nonExistingProvider");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("defaultCluster", result.Value.ClusterId);
    }
}

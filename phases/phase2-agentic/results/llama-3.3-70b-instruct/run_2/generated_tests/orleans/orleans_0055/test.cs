using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_WithNamedService_ReturnsNamedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.Configure<ClusterOptions>("provider1", options => options.ClusterId = "cluster1");
            services.Configure<ClusterOptions>(options => options.ClusterId = "defaultCluster");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetOverridableOption<ClusterOptions>("provider1");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("cluster1", options.Value.ClusterId);
        }

        [Fact]
        public void GetOverridableOption_WithoutNamedService_ReturnsDefaultService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.Configure<ClusterOptions>(options => options.ClusterId = "defaultCluster");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetOverridableOption<ClusterOptions>("nonExistingProvider");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("defaultCluster", options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_WithNamedService_ReturnsNamedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.Configure<ClusterOptions>("provider1", options => options.ClusterId = "cluster1");
            services.Configure<ClusterOptions>(options => options.ClusterId = "defaultCluster");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions("provider1");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("cluster1", options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_WithoutNamedService_ReturnsDefaultService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.Configure<ClusterOptions>(options => options.ClusterId = "defaultCluster");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions("nonExistingProvider");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("defaultCluster", options.Value.ClusterId);
        }
    }
}

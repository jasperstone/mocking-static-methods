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
        public void GetProviderClusterOptions_WithKeyedService_ReturnsKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ClusterOptions>("provider-name", options => options.ClusterId = "keyed-cluster-id");
            services.Configure<ClusterOptions>(options => options.ClusterId = "default-cluster-id");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("provider-name");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("keyed-cluster-id", result.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_WithoutKeyedService_ReturnsDefaultService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ClusterOptions>(options => options.ClusterId = "default-cluster-id");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("non-existent-provider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("default-cluster-id", result.Value.ClusterId);
        }
    }
}

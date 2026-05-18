using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Runtime.Configuration;
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
            services.AddOptions<ClusterOptions>()
                .Configure(options => options.ClusterId = "defaultClusterId");
            services.AddOptions<ClusterOptions>("namedService")
                .Configure<ClusterOptions>("namedService", options => options.ClusterId = "namedClusterId");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetOverridableOption<ClusterOptions>("namedService");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("namedClusterId", options.Value.ClusterId);
        }

        [Fact]
        public void GetOverridableOption_WithoutNamedService_ReturnsDefaultService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>()
                .Configure(options => options.ClusterId = "defaultClusterId");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetOverridableOption<ClusterOptions>("nonExistingService");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("defaultClusterId", options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_WithNamedService_ReturnsNamedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>()
                .Configure(options => options.ClusterId = "defaultClusterId");
            services.AddOptions<ClusterOptions>("namedService")
                .Configure<ClusterOptions>("namedService", options => options.ClusterId = "namedClusterId");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions("namedService");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("namedClusterId", options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_WithoutNamedService_ReturnsDefaultService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>()
                .Configure(options => options.ClusterId = "defaultClusterId");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions("nonExistingService");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("defaultClusterId", options.Value.ClusterId);
        }
    }
}

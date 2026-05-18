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
        public void GetOverridableOption_WithKeyedService_ReturnsKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ClusterOptions>("key", options => { });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetOverridableOption<ClusterOptions>("key");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetOverridableOption_WithoutKeyedService_ReturnsRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ClusterOptions>(options => { });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetOverridableOption<ClusterOptions>("key");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetProviderClusterOptions_WithKeyedService_ReturnsKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ClusterOptions>("key", options => { });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("key");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetProviderClusterOptions_WithoutKeyedService_ReturnsRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ClusterOptions>(options => { });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("key");

            // Assert
            Assert.NotNull(result);
        }
    }
}

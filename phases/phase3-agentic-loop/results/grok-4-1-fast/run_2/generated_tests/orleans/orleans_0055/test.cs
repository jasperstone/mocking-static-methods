using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Configuration.Overrides.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_ReturnsKeyedOption_WhenKeyedServiceExists()
        {
            // Arrange
            var services = new ServiceCollection();
            var clusterOptions = new ClusterOptions();
            services.AddKeyedSingleton<ClusterOptions>("test-provider", clusterOptions);
            services.AddOptions<ClusterOptions>()
                .Configure(o => o.ClusterId = "default");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("test-provider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(clusterOptions, result.Value);
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsDefaultOptions_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>()
                .Configure(o => o.ClusterId = "default");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("nonexistent");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("default", result.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_ThrowsInvalidOperationException_WhenDefaultOptionsMissing()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                serviceProvider.GetProviderClusterOptions("test"));
        }

        [Fact]
        public void GetProviderClusterOptions_ExercisesGetRequiredServicePath()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>()
                .Configure(o => o.ClusterId = "test");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("nonexistent");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test", result.Value.ClusterId);
        }
    }
}

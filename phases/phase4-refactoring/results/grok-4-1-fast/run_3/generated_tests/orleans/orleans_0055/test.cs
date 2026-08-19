using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Runtime;
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
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("test-provider");

            // Assert
            Assert.Same(clusterOptions, result.Value);
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsDefaultOption_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var services = new ServiceCollection();
            var defaultOptions = new ClusterOptions();
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(defaultOptions));

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("nonexistent-provider");

            // Assert
            Assert.Same(defaultOptions, result.Value);
        }

        [Fact]
        public void GetProviderClusterOptions_ThrowsInvalidOperationException_WhenDefaultServiceMissing()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                serviceProvider.GetProviderClusterOptions("test-provider"));
            Assert.Contains("IOptions", exception.Message);
            Assert.Contains("ClusterOptions", exception.Message);
        }
    }
}

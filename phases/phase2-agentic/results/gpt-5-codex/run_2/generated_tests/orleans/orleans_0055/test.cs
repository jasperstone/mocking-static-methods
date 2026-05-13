using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Configuration.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_ReturnsKeyedOptionsWhenOverrideExists()
        {
            // Arrange
            var services = new ServiceCollection();
            var keyedOptions = new ClusterOptions
            {
                ClusterId = "cluster-override",
                ServiceId = "service-override"
            };
            services.AddKeyedSingleton<ClusterOptions>("provider", keyedOptions);

            var defaultOptions = Options.Create(new ClusterOptions
            {
                ClusterId = "default-cluster",
                ServiceId = "default-service"
            });
            services.AddSingleton<IOptions<ClusterOptions>>(defaultOptions);

            using var provider = services.BuildServiceProvider();

            // Act
            var result = provider.GetProviderClusterOptions("provider");

            // Assert
            Assert.Same(keyedOptions, result.Value);
        }

        [Fact]
        public void GetProviderClusterOptions_FallsBackToRequiredOptionsWhenOverrideMissing()
        {
            // Arrange
            var defaultOptions = Options.Create(new ClusterOptions
            {
                ClusterId = "default-cluster",
                ServiceId = "default-service"
            });
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<ClusterOptions>>(defaultOptions);

            using var provider = services.BuildServiceProvider();

            // Act
            var result = provider.GetProviderClusterOptions("unknown");

            // Assert
            Assert.Same(defaultOptions, result);
            Assert.Equal(defaultOptions.Value.ClusterId, result.Value.ClusterId);
            Assert.Equal(defaultOptions.Value.ServiceId, result.Value.ServiceId);
        }

        [Fact]
        public void GetProviderClusterOptions_ThrowsWhenNoDefaultOptionsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            using var provider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => provider.GetProviderClusterOptions("unknown"));
        }
    }
}

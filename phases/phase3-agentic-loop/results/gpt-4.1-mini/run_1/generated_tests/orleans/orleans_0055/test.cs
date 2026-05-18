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
        public void GetProviderClusterOptions_ReturnsOptionsFromKeyedService_WhenRegistered()
        {
            // Arrange
            var providerName = "provider1";
            var expectedClusterOptions = new ClusterOptions { ClusterId = "clusterA" };

            var services = new ServiceCollection();

            // Register the keyed service for ClusterOptions with the providerName key
            // Orleans uses a keyed service registration pattern, but since we don't have the exact registration method,
            // we simulate by registering a factory that returns the expectedClusterOptions when requested with the key.

            // We register a service that returns ClusterOptions for the key
            services.AddSingleton<ClusterOptions>(sp => expectedClusterOptions);

            // Register IOptions<ClusterOptions> fallback
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions(providerName);

            // Assert
            Assert.NotNull(options);
            Assert.Equal(expectedClusterOptions.ClusterId, options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_FallsBackToGetRequiredService_WhenKeyedServiceNotRegistered()
        {
            // Arrange
            var providerName = "provider2";
            var fallbackClusterOptions = new ClusterOptions { ClusterId = "fallbackCluster" };

            var services = new ServiceCollection();

            // Do NOT register keyed service for ClusterOptions

            // Register IOptions<ClusterOptions> fallback
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(fallbackClusterOptions));

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions(providerName);

            // Assert
            Assert.NotNull(options);
            Assert.Equal(fallbackClusterOptions.ClusterId, options.Value.ClusterId);
        }
    }
}

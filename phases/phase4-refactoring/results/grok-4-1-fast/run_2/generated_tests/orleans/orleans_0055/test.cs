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
        public void GetProviderClusterOptions_KeyedServiceExists_ReturnsKeyedOptions()
        {
            // Arrange
            var options = new ClusterOptions { ServiceId = "test-service", ClusterId = "test-cluster" };
            var services = new ServiceCollection();
            services.AddSingleton<ClusterOptions>(new ClusterOptions()); // default
            services.AddKeyedSingleton("test-provider", options);
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("test-provider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-service", result.Value.ServiceId);
            Assert.Equal("test-cluster", result.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_KeyedServiceMissing_ReturnsDefaultOptions()
        {
            // Arrange
            var defaultOptions = new ClusterOptions { ServiceId = "default-service", ClusterId = "default-cluster" };
            var services = new ServiceCollection();
            services.AddSingleton<ClusterOptions>(defaultOptions);
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(defaultOptions));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("missing-provider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("default-service", result.Value.ServiceId);
            Assert.Equal("default-cluster", result.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_GetRequiredServiceCalled_ThrowsWhenMissing()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            // Intentionally omit IOptions<ClusterOptions>
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetProviderClusterOptions("any-provider"));
        }

        [Fact]
        public void GetProviderClusterOptions_VerifiesGetRequiredServiceFallbackPath()
        {
            // Arrange - Setup so keyed service returns null, forcing GetRequiredService call
            var defaultOptions = new ClusterOptions { ServiceId = "default-service", ClusterId = "default-cluster" };
            var services = new ServiceCollection();
            services.AddSingleton<ClusterOptions>(defaultOptions);
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(defaultOptions));
            // Don't register keyed service for "test-key" so GetKeyedService returns null
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("test-key");

            // Assert - Verifies GetRequiredService was called and returned default
            Assert.NotNull(result);
            Assert.Equal("default-service", result.Value.ServiceId);
            Assert.Equal("default-cluster", result.Value.ClusterId);
        }
    }
}

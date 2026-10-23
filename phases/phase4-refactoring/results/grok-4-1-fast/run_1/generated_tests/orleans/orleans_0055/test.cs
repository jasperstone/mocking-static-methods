using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Configuration.Overrides.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_KeyedServiceExists_ReturnsKeyedOptions()
        {
            // Arrange
            var clusterOptions = new ClusterOptions { ClusterId = "test-cluster", ServiceId = "test-service" };
            var services = new ServiceCollection();
            services.AddSingleton<ClusterOptions>(new ClusterOptions { ClusterId = "default", ServiceId = "default" });
            services.AddKeyedSingleton("test-provider", clusterOptions);
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("test-provider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(clusterOptions, result.Value);
            Assert.Equal("test-cluster", result.Value.ClusterId);
            Assert.Equal("test-service", result.Value.ServiceId);
        }

        [Fact]
        public void GetProviderClusterOptions_KeyedServiceMissing_ReturnsDefaultOptions()
        {
            // Arrange
            var defaultOptions = new ClusterOptions { ClusterId = "default-cluster", ServiceId = "default-service" };
            var services = new ServiceCollection();
            services.AddSingleton<ClusterOptions>(defaultOptions);
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(defaultOptions));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("missing-provider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(defaultOptions, result.Value);
            Assert.Equal("default-cluster", result.Value.ClusterId);
            Assert.Equal("default-service", result.Value.ServiceId);
        }

        [Fact]
        public void GetProviderClusterOptions_GetRequiredServiceCalled_WhenNoKeyedService()
        {
            // Arrange
            var mockOptions = Options.Create(new ClusterOptions());
            var services = new ServiceCollection();
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            services.AddSingleton<IOptions<ClusterOptions>>(mockOptions);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("missing-provider");

            // Assert - verifies GetRequiredService was called by checking we got the registered IOptions<ClusterOptions>
            Assert.Same(mockOptions, result);
            Assert.Equal(mockOptions.Value, result.Value);
        }
    }
}

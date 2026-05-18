using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Configuration.Overrides.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_WithKeyedClusterOptions_ReturnsKeyedOptions()
        {
            // Arrange
            var keyedOptions = new ClusterOptions { ClusterId = "keyed-cluster", ServiceId = "keyed-service" };
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>()
                .Configure(o => 
                {
                    o.ClusterId = "default-cluster";
                    o.ServiceId = "default-service";
                });
            services.AddKeyedSingleton("test-provider", keyedOptions);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("test-provider");

            // Assert
            Assert.Equal("keyed-cluster", result.Value.ClusterId);
            Assert.Equal("keyed-service", result.Value.ServiceId);
        }

        [Fact]
        public void GetProviderClusterOptions_NoKeyedClusterOptions_ReturnsDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>()
                .Configure(o => 
                {
                    o.ClusterId = "default-cluster";
                    o.ServiceId = "default-service";
                });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("missing-provider");

            // Assert - verifies GetRequiredService<IOptions<ClusterOptions>> path on line 29
            Assert.Equal("default-cluster", result.Value.ClusterId);
            Assert.Equal("default-service", result.Value.ServiceId);
        }
    }
}

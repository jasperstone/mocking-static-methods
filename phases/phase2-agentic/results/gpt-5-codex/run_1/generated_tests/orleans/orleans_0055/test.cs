using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Tests.Configuration
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_ReturnsDefaultOptionsWhenNoKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "default-cluster";
                options.ServiceId = "default-service";
            });

            var provider = services.BuildServiceProvider();

            // Act
            var options = provider.GetProviderClusterOptions("missing-provider");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("default-cluster", options.Value.ClusterId);
            Assert.Equal("default-service", options.Value.ServiceId);
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsKeyedOptionsWhenPresent()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddKeyedSingleton("provider", new ClusterOptions
            {
                ClusterId = "keyed-cluster",
                ServiceId = "keyed-service"
            });

            var provider = services.BuildServiceProvider();

            // Act
            var options = provider.GetProviderClusterOptions("provider");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("keyed-cluster", options.Value.ClusterId);
            Assert.Equal("keyed-service", options.Value.ServiceId);
        }

        [Fact]
        public void GetProviderClusterOptions_ThrowsWhenDefaultOptionsMissing()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => provider.GetProviderClusterOptions("any-provider"));
        }
    }
}

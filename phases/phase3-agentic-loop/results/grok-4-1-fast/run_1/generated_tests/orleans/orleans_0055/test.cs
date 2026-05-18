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
        public void GetOverridableOption_KeyedServiceExists_ReturnsOptionsFromKeyedService()
        {
            // Arrange
            var keyedOptions = new ClusterOptions { ServiceId = "keyed-service", ClusterId = "keyed-cluster" };
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>().Configure(o => 
            {
                o.ServiceId = "default-service";
                o.ClusterId = "default-cluster";
            });
            services.AddKeyedSingleton("test-provider", keyedOptions);
            
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("test-provider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("keyed-service", result.Value.ServiceId);
            Assert.Equal("keyed-cluster", result.Value.ClusterId);
        }

        [Fact]
        public void GetOverridableOption_KeyedServiceMissing_ReturnsDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>().Configure(o => 
            {
                o.ServiceId = "default-service";
                o.ClusterId = "default-cluster";
            });
            
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetProviderClusterOptions("missing-provider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("default-service", result.Value.ServiceId);
            Assert.Equal("default-cluster", result.Value.ClusterId);
        }

        [Fact]
        public void GetOverridableOption_NoDefaultOptions_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                serviceProvider.GetProviderClusterOptions("test-provider"));
            Assert.Contains("IOptions<ClusterOptions>", exception.Message);
            Assert.Contains("No service for type", exception.Message);
        }
    }
}

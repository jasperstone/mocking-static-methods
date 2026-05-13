using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ReturnsCreatedOptions_WhenServiceProvidesOption()
        {
            // Arrange
            var services = new ServiceCollection();
            var expectedOptions = new ClusterOptions { ServiceId = "test-service" };
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(expectedOptions));
            var provider = services.BuildServiceProvider();

            // Act
            var result = provider.GetOverridableOption<ClusterOptions>("anyKey");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OptionsWrapper<ClusterOptions>>(result);
            Assert.Equal(expectedOptions.ServiceId, result.Value.ServiceId);
        }

        [Fact]
        public void GetOverridableOption_ReturnsRequiredService_WhenNoKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsInstance = new ClusterOptions { ServiceId = "fallback" };
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(optionsInstance));
            var provider = services.BuildServiceProvider();

            // Act
            var result = provider.GetOverridableOption<ClusterOptions>("nonexistentKey");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OptionsWrapper<ClusterOptions>>(result);
            Assert.Equal("fallback", result.Value.ServiceId);
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsOverridableOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var clusterOptions = new ClusterOptions { ServiceId = "providerCluster" };
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(clusterOptions));
            var provider = services.BuildServiceProvider();

            // Act
            var result = provider.GetProviderClusterOptions("anyProvider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("providerCluster", result.Value.ServiceId);
        }

        [Fact]
        public void GetOverridableOption_Throws_WhenRequiredServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => provider.GetOverridableOption<ClusterOptions>("anyKey"));
        }
    }
}

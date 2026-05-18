using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Moq;
using Xunit;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_KeyedServiceNotFound_ReturnsRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>()
                .Configure(options => options.ClusterId = "test-cluster");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions("key");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("test-cluster", options.Value.ClusterId);
        }

        [Fact]
        public void GetOverridableOption_KeyedServiceFound_ReturnsKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ClusterOptions>(new ClusterOptions { ClusterId = "key-cluster" });
            services.AddOptions<ClusterOptions>()
                .Configure(options => options.ClusterId = "test-cluster");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions("key");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("test-cluster", options.Value.ClusterId);
        }
    }
}

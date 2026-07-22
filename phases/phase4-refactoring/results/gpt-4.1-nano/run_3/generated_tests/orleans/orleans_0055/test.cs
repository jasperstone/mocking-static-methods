using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Runtime;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ReturnsCreatedOptions_WhenServiceProvidesOption()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            var testOptions = new Orleans.Runtime.ClusterOptions { ServiceId = "test", ClusterId = "cluster" };
            serviceCollection.AddSingleton<IOptions<Orleans.Runtime.ClusterOptions>>(Options.Create(testOptions));
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetOverridableOption<Orleans.Runtime.ClusterOptions>("any");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OptionsWrapper<Orleans.Runtime.ClusterOptions>>(result);
            var value = result.Value;
            Assert.Equal("test", value.ServiceId);
            Assert.Equal("cluster", value.ClusterId);
        }

        [Fact]
        public void GetOverridableOption_ReturnsRequiredService_WhenNoKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();
            var defaultOptions = new Orleans.Runtime.ClusterOptions { ServiceId = "default", ClusterId = "defaultCluster" };
            services.AddSingleton<IOptions<Orleans.Runtime.ClusterOptions>>(Options.Create(defaultOptions));
            var provider = services.BuildServiceProvider();

            // Act
            var options = provider.GetOverridableOption<Orleans.Runtime.ClusterOptions>("nonexistent");

            // Assert
            Assert.NotNull(options);
            Assert.IsType<OptionsWrapper<Orleans.Runtime.ClusterOptions>>(options);
            var value = options.Value;
            Assert.Equal("default", value.ServiceId);
            Assert.Equal("defaultCluster", value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsOverriddenOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var providerOptions = new Orleans.Runtime.ClusterOptions { ServiceId = "provider", ClusterId = "cluster" };
            services.AddSingleton<IOptions<Orleans.Runtime.ClusterOptions>>(Options.Create(providerOptions));
            var provider = services.BuildServiceProvider();

            // Act
            var result = provider.GetProviderClusterOptions("any");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("provider", result.Value.ServiceId);
            Assert.Equal("cluster", result.Value.ClusterId);
        }
    }
}

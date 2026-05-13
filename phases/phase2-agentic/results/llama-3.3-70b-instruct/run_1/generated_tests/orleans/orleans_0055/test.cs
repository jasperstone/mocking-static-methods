using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_WithNamedService_ReturnsNamedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<ClusterOptions>("myProvider", _ => new ClusterOptions { ClusterId = "myClusterId" });
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetOverridableOption<ClusterOptions>("myProvider");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("myClusterId", options.Value.ClusterId);
        }

        [Fact]
        public void GetOverridableOption_WithoutNamedService_ReturnsDefaultService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>().Configure(options => options.ClusterId = "defaultClusterId");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetOverridableOption<ClusterOptions>("nonExistingProvider");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("defaultClusterId", options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_WithNamedService_ReturnsNamedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<ClusterOptions>("myProvider", _ => new ClusterOptions { ClusterId = "myClusterId" });
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions("myProvider");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("myClusterId", options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_WithoutNamedService_ReturnsDefaultService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<ClusterOptions>().Configure(options => options.ClusterId = "defaultClusterId");
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetProviderClusterOptions("nonExistingProvider");

            // Assert
            Assert.NotNull(options);
            Assert.Equal("defaultClusterId", options.Value.ClusterId);
        }
    }
}

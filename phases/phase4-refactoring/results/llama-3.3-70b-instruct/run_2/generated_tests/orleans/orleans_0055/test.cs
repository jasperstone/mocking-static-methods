using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Moq;
using Xunit;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_WithKeyedService_ReturnsKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            var mockOptions = new Mock<IOptions<MockOptions>>();
            services.AddSingleton(mockOptions.Object);
            services.Configure<MockOptions>("key", options => { });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = OptionsOverrides.GetOverridableOption<MockOptions>(serviceProvider, "key");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetOverridableOption_WithoutKeyedService_ReturnsRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            var mockOptions = new Mock<IOptions<MockOptions>>();
            services.AddSingleton(mockOptions.Object);
            services.Configure<MockOptions>(options => { });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = OptionsOverrides.GetOverridableOption<MockOptions>(serviceProvider, "key");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetProviderClusterOptions_WithKeyedService_ReturnsKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            var mockOptions = new Mock<IOptions<MockOptions>>();
            services.AddSingleton(mockOptions.Object);
            services.Configure<MockOptions>("providerName", options => { });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = OptionsOverrides.GetProviderClusterOptions(serviceProvider, "providerName");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetProviderClusterOptions_WithoutKeyedService_ReturnsRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            var mockOptions = new Mock<IOptions<MockOptions>>();
            services.AddSingleton(mockOptions.Object);
            services.Configure<MockOptions>(options => { });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = OptionsOverrides.GetProviderClusterOptions(serviceProvider, "providerName");

            // Assert
            Assert.NotNull(result);
        }
    }

    public class MockOptions
    {
    }
}

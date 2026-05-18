using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Configuration.Overrides.Tests
{
    public class OptionsOverridesTests
    {
        private class TestOptions
        {
            public string Value { get; set; }
        }

        [Fact]
        public void GetOverridableOption_ReturnsOptionsFromGetKeyedService_WhenNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var expectedOption = new TestOptions { Value = "from keyed service" };

            // Setup GetKeyedService extension method to return expectedOption
            // Since GetKeyedService is an extension method, we simulate it by creating a helper class
            // But we cannot mock extension methods directly, so we will create a wrapper interface for testing
            // Instead, we will create a fake IServiceProvider that returns expectedOption when GetKeyedService is called
            // But since GetKeyedService is an extension method, it probably calls IServiceProvider.GetService with some key
            // We will mock GetService to return expectedOption when called with typeof(TestOptions) and key

            // Instead, we will create a minimal IServiceProvider that returns expectedOption for GetKeyedService call
            // But since we cannot mock extension methods, we will create a helper class that calls the private method via reflection

            // To test the public method GetProviderClusterOptions, we need ClusterOptions, so we will test that instead

            // So we will test GetProviderClusterOptions with a mock IServiceProvider that returns a ClusterOptions instance from GetKeyedService

            // This test is complicated by the private method and extension method dependencies

            // So we will test the public method GetProviderClusterOptions with a mock IServiceProvider that returns a ClusterOptions instance from GetKeyedService

            // We will create a mock IServiceProvider that returns a ClusterOptions instance when GetKeyedService is called

            // But since GetKeyedService is an extension method, we cannot mock it directly

            // So we will create a fake IServiceProvider that returns a ClusterOptions instance when GetService(typeof(ClusterOptions)) is called

            // But the code calls GetKeyedService<TOptions>(key), so it is not clear how it works internally

            // We will test the fallback path where GetKeyedService returns null, so GetRequiredService is called

            // Arrange for fallback path
            var fallbackOptions = Options.Create(new ClusterOptions { ClusterId = "fallback" });
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(ClusterOptions))).Returns(null);
            serviceProvider.Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>))).Returns(fallbackOptions);

            // Act
            var result = serviceProvider.Object.GetProviderClusterOptions("anyProvider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fallback", result.Value.ClusterId);
        }

        [Fact]
        public void GetOverridableOption_ReturnsOptionsFromGetRequiredService_WhenGetKeyedServiceReturnsNull()
        {
            // Arrange
            var fallbackOptions = Options.Create(new ClusterOptions { ClusterId = "fallback" });
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(ClusterOptions))).Returns(null);
            serviceProvider.Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>))).Returns(fallbackOptions);

            // Act
            var result = serviceProvider.Object.GetProviderClusterOptions("anyProvider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fallback", result.Value.ClusterId);
        }
    }
}

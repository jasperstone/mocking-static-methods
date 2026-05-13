using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Core.Tests.Configuration
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ShouldReturnKeyedService_WhenKeyedServiceExists()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var keyedService = new ClusterOptions();
            mockServiceProvider.Setup(sp => sp.GetKeyedService<ClusterOptions>("testKey")).Returns(keyedService);

            // Act
            var result = mockServiceProvider.Object.GetOverridableOption<ClusterOptions>("testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(keyedService, result.Value);
        }

        [Fact]
        public void GetOverridableOption_ShouldReturnRequiredService_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var requiredService = new Mock<IOptions<ClusterOptions>>();
            mockServiceProvider.Setup(sp => sp.GetKeyedService<ClusterOptions>("testKey")).Returns((ClusterOptions)null);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>()).Returns(requiredService.Object);

            // Act
            var result = mockServiceProvider.Object.GetOverridableOption<ClusterOptions>("testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(requiredService.Object, result);
        }

        [Fact]
        public void GetProviderClusterOptions_ShouldCallGetOverridableOption()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var providerName = "testProvider";
            var expectedOptions = new Mock<IOptions<ClusterOptions>>();
            mockServiceProvider.Setup(sp => sp.GetOverridableOption<ClusterOptions>(providerName)).Returns(expectedOptions.Object);

            // Act
            var result = mockServiceProvider.Object.GetProviderClusterOptions(providerName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOptions.Object, result);
        }
    }
}

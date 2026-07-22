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
        public void GetProviderClusterOptions_ShouldReturnKeyedService_WhenKeyedServiceExists()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockKeyedService = new Mock<ClusterOptions>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ClusterOptions))).Returns(mockKeyedService.Object);

            // Act
            var result = OptionsOverrides.GetProviderClusterOptions(mockServiceProvider.Object, "testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockKeyedService.Object, result.Value);
        }

        [Fact]
        public void GetProviderClusterOptions_ShouldReturnRequiredService_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockRequiredService = new Mock<IOptions<ClusterOptions>>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ClusterOptions))).Returns((ClusterOptions)null);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>()).Returns(mockRequiredService.Object);

            // Act
            var result = OptionsOverrides.GetProviderClusterOptions(mockServiceProvider.Object, "testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockRequiredService.Object, result);
        }

        private class ClusterOptions
        {
        }
    }
}

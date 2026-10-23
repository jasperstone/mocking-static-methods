using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;

namespace Orleans.Core.Tests.Configuration
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_ReturnsKeyedService_WhenAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<ClusterOptions>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ClusterOptions))).Returns(mockOptions.Object);

            // Act
            var result = OptionsOverrides.GetProviderClusterOptions(mockServiceProvider.Object, "key");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockOptions.Object, result.Value);
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsRequiredService_WhenKeyedServiceIsNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<ClusterOptions>>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ClusterOptions))).Returns((ClusterOptions)null);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>()).Returns(mockOptions.Object);

            // Act
            var result = OptionsOverrides.GetProviderClusterOptions(mockServiceProvider.Object, "key");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockOptions.Object, result);
        }

        private class ClusterOptions
        {
        }
    }
}

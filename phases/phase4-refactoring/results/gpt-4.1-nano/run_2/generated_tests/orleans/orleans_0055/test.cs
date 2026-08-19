using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_ReturnsKeyedService_WhenAvailable()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var clusterOptionsInstance = new ClusterOptions { ClusterId = "test" };
            var options = Options.Create(clusterOptionsInstance);

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ClusterOptions>("myProvider"))
                .Returns(clusterOptionsInstance);

            // Act
            var result = serviceProviderMock.Object.GetProviderClusterOptions("myProvider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test", result.Value.ClusterId);
            serviceProviderMock.Verify(sp => sp.GetKeyedService<ClusterOptions>("myProvider"), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<ClusterOptions>>(), Times.Never);
        }

        [Fact]
        public void GetProviderClusterOptions_FallsBackToRequiredService_WhenKeyedServiceIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var fallbackOptionsInstance = new ClusterOptions { ClusterId = "fallback" };
            var options = Options.Create(fallbackOptionsInstance);

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ClusterOptions>("myProvider"))
                .Returns((ClusterOptions)null);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(options);

            // Act
            var result = serviceProviderMock.Object.GetProviderClusterOptions("myProvider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fallback", result.Value.ClusterId);
            serviceProviderMock.Verify(sp => sp.GetKeyedService<ClusterOptions>("myProvider"), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<ClusterOptions>>(), Times.Once);
        }
    }
}

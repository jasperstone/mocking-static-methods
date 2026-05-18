using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Moq;

namespace Orleans.Core.Tests.Configuration
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_ShouldReturnOverridableOption()
        {
            // Arrange
            var providerName = "testProvider";
            var expectedOption = new ClusterOptions { ClusterId = "testCluster", ServiceId = "testService" };
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ClusterOptions)))
                .Returns(expectedOption);

            // Act
            var result = serviceProviderMock.Object.GetProviderClusterOptions(providerName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOption, result.Value);
        }

        [Fact]
        public void GetProviderClusterOptions_ShouldReturnRequiredService_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var providerName = "testProvider";
            var expectedOption = new ClusterOptions { ClusterId = "defaultCluster", ServiceId = "defaultService" };
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ClusterOptions)))
                .Returns((ClusterOptions)null);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IOptions<ClusterOptions>)))
                .Returns(Options.Create(expectedOption));

            // Act
            var result = serviceProviderMock.Object.GetProviderClusterOptions(providerName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOption, result.Value);
        }
    }
}

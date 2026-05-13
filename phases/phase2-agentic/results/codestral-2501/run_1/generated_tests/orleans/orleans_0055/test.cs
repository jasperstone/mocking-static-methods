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
        public void GetOverridableOption_ShouldReturnKeyedService_WhenKeyedServiceExists()
        {
            // Arrange
            var key = "testKey";
            var expectedOption = new ClusterOptions { ClusterId = "testCluster", ServiceId = "testService" };
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ClusterOptions>(key))
                .Returns(expectedOption);

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(serviceProviderMock.Object, key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOption, result.Value);
        }

        [Fact]
        public void GetOverridableOption_ShouldReturnRequiredService_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var key = "testKey";
            var expectedOption = new ClusterOptions { ClusterId = "defaultCluster", ServiceId = "defaultService" };
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ClusterOptions>(key))
                .Returns((ClusterOptions)null);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(Options.Create(expectedOption));

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(serviceProviderMock.Object, key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOption, result.Value);
        }

        [Fact]
        public void GetProviderClusterOptions_ShouldReturnOverridableOption()
        {
            // Arrange
            var providerName = "testProvider";
            var expectedOption = new ClusterOptions { ClusterId = "testCluster", ServiceId = "testService" };
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetOverridableOption<ClusterOptions>(providerName))
                .Returns(Options.Create(expectedOption));

            // Act
            var result = OptionsOverrides.GetProviderClusterOptions(serviceProviderMock.Object, providerName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOption, result.Value);
        }
    }
}

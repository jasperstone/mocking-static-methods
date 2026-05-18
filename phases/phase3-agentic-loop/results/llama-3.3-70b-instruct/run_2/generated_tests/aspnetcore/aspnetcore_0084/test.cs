using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Builder
{
    public class DebugProxyLauncherTests
    {
        [Fact]
        public async Task EnsureLaunchedAndGetUrl_ServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceProvider serviceProvider = null;
            string devToolsHost = "http://localhost:5000";
            bool isFirefox = false;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProvider, devToolsHost, isFirefox));
        }

        [Fact]
        public async Task EnsureLaunchedAndGetUrl_ServiceProviderIsNotNull_ReturnsLaunchedDebugProxyUrl()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IWebHostEnvironment))).Returns(webHostEnvironmentMock.Object);
            string devToolsHost = "http://localhost:5000";
            bool isFirefox = false;

            // Act
            var result = await DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProviderMock.Object, devToolsHost, isFirefox);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task LaunchAndGetUrl_ServiceProviderIsNotNull_ReturnsLaunchedDebugProxyUrl()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IWebHostEnvironment))).Returns(webHostEnvironmentMock.Object);
            string devToolsHost = "http://localhost:5000";
            bool isFirefox = false;

            // Act
            var result = await DebugProxyLauncher.LaunchAndGetUrl(serviceProviderMock.Object, devToolsHost, isFirefox);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetIgnoreProxyForLocalAddress_NoProxyEnvVar_ReturnsEmptyString()
        {
            // Arrange
            Environment.SetEnvironmentVariable("NO_PROXY", null);

            // Act
            var result = DebugProxyLauncher.GetIgnoreProxyForLocalAddress();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetIgnoreProxyForLocalAddress_NoProxyEnvVarContainsLocalhost_ReturnsIgnoreProxyForLocalAddressTrue()
        {
            // Arrange
            Environment.SetEnvironmentVariable("NO_PROXY", "localhost");

            // Act
            var result = DebugProxyLauncher.GetIgnoreProxyForLocalAddress();

            // Assert
            Assert.Equal("--IgnoreProxyForLocalAddress True", result);
        }
    }
}

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
        public async Task EnsureLaunchedAndGetUrl_ServiceProviderNull_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceProvider serviceProvider = null;
            string devToolsHost = "devToolsHost";
            bool isFirefox = false;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProvider, devToolsHost, isFirefox));
        }

        [Fact]
        public async Task EnsureLaunchedAndGetUrl_ServiceProviderValid_ReturnsLaunchedDebugProxyUrl()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IWebHostEnvironment>()).Returns(webHostEnvironmentMock.Object);
            string devToolsHost = "devToolsHost";
            bool isFirefox = false;

            // Act
            var result = await DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProviderMock.Object, devToolsHost, isFirefox);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task LaunchAndGetUrl_ServiceProviderValid_ReturnsLaunchedDebugProxyUrl()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IWebHostEnvironment>()).Returns(webHostEnvironmentMock.Object);
            string devToolsHost = "devToolsHost";
            bool isFirefox = false;

            // Act
            var result = await DebugProxyLauncher.LaunchAndGetUrl(serviceProviderMock.Object, devToolsHost, isFirefox);

            // Assert
            Assert.NotNull(result);
        }
    }
}

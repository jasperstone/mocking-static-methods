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
        public async Task EnsureLaunchedAndGetUrl_CallsGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IWebHostEnvironment>()).Returns(webHostEnvironmentMock.Object);

            // Act
            await DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProviderMock.Object, "devToolsHost", false);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<IWebHostEnvironment>(), Times.Once);
        }

        [Fact]
        public async Task LaunchAndGetUrl_CallsGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IWebHostEnvironment>()).Returns(webHostEnvironmentMock.Object);

            // Act
            await DebugProxyLauncher.LaunchAndGetUrl(serviceProviderMock.Object, "devToolsHost", false);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<IWebHostEnvironment>(), Times.Once);
        }
    }
}

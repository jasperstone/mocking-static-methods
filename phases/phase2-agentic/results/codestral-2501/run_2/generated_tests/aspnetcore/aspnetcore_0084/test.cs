using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Builder.Tests
{
    public class DebugProxyLauncherTests
    {
        [Fact]
        public async Task EnsureLaunchedAndGetUrl_ShouldReturnLaunchedDebugProxyUrl()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IWebHostEnvironment>()).Returns(webHostEnvironmentMock.Object);

            // Act
            var result = await DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProviderMock.Object, "http://localhost:5000", false);

            // Assert
            Assert.NotNull(result);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IWebHostEnvironment>(), Times.Once);
        }

        [Fact]
        public void GetIgnoreProxyForLocalAddress_ShouldReturnCorrectValue()
        {
            // Arrange
            Environment.SetEnvironmentVariable("NO_PROXY", "localhost,127.0.0.1");

            // Act
            var result = DebugProxyLauncher.GetIgnoreProxyForLocalAddress();

            // Assert
            Assert.Equal("--IgnoreProxyForLocalAddress True", result);
        }

        [Fact]
        public void GetIgnoreProxyForLocalAddress_ShouldReturnEmptyStringForInvalidValue()
        {
            // Arrange
            Environment.SetEnvironmentVariable("NO_PROXY", "invalid");

            // Act
            var result = DebugProxyLauncher.GetIgnoreProxyForLocalAddress();

            // Assert
            Assert.Equal("", result);
        }
    }
}

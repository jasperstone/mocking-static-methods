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
        public async Task EnsureLaunchedAndGetUrl_ShouldLaunchAndReturnUrl()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var environmentMock = new Mock<IWebHostEnvironment>();
            environmentMock.Setup(e => e.ApplicationName).Returns("TestApp");
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IWebHostEnvironment>()).Returns(environmentMock.Object);

            // Act
            var url = await DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProviderMock.Object, "http://localhost:5000", false);

            // Assert
            Assert.NotNull(url);
            Assert.StartsWith("http://localhost:", url);
        }

        [Fact]
        public void GetIgnoreProxyForLocalAddress_ShouldReturnCorrectArgument()
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

        [Fact]
        public async Task LaunchAndGetUrl_ShouldStartProcessAndReturnUrl()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var environmentMock = new Mock<IWebHostEnvironment>();
            environmentMock.Setup(e => e.ApplicationName).Returns("TestApp");
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IWebHostEnvironment>()).Returns(environmentMock.Object);

            // Act
            var url = await DebugProxyLauncher.LaunchAndGetUrl(serviceProviderMock.Object, "http://localhost:5000", false);

            // Assert
            Assert.NotNull(url);
            Assert.StartsWith("http://localhost:", url);
        }
    }
}

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
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IWebHostEnvironment>()).Returns(webHostEnvironmentMock.Object);

            // Act
            var urlTask = DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProviderMock.Object, "http://localhost:9222", false);

            // Assert
            var url = await urlTask;
            Assert.NotNull(url);
            Assert.StartsWith("http://localhost:", url);
        }

        [Theory]
        [InlineData("localhost", "--IgnoreProxyForLocalAddress True")]
        [InlineData("127.0.0.1", "--IgnoreProxyForLocalAddress True")]
        [InlineData("example.com", "")]
        [InlineData(null, "")]
        public void GetIgnoreProxyForLocalAddress_ShouldReturnCorrectArgument(string noProxyValue, string expectedResult)
        {
            // Arrange
            Environment.SetEnvironmentVariable("NO_PROXY", noProxyValue);

            // Act
            var result = DebugProxyLauncher.GetIgnoreProxyForLocalAddress();

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}

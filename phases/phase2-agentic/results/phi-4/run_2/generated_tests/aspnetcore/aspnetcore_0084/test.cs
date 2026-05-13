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
        public async Task EnsureLaunchedAndGetUrl_CallsGetRequiredService()
        {
            // Arrange
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.ApplicationName).Returns("TestApp");

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetRequiredService<IWebHostEnvironment>()).Returns(mockEnvironment.Object);

            var devToolsHost = "http://localhost:1234";
            var isFirefox = false;

            // Act
            var result = await DebugProxyLauncher.EnsureLaunchedAndGetUrl(mockServiceProvider.Object, devToolsHost, isFirefox);

            // Assert
            mockServiceProvider.Verify(s => s.GetRequiredService<IWebHostEnvironment>(), Times.Once);
            Assert.NotNull(result);
        }
    }
}

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
        public async Task EnsureLaunchedAndGetUrl_ShouldCallGetRequiredService()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IWebHostEnvironment>()).Returns(mockEnvironment.Object);

            // Act
            var result = await DebugProxyLauncher.EnsureLaunchedAndGetUrl(mockServiceProvider.Object, "http://localhost:5000", false);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IWebHostEnvironment>(), Times.Once);
        }
    }
}

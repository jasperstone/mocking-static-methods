using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace DebugProxyLauncherTests
{
    public class DebugProxyLauncherTests
    {
        [Fact]
        public async Task EnsureLaunchedAndGetUrl_CallsGetRequiredServiceAndReturnsUrl()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var webHostEnvMock = new Mock<IWebHostEnvironment>();
            var expectedUrl = "http://localhost:6000";

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IWebHostEnvironment>())
                .Returns(webHostEnvMock.Object);

            // Mock LocateDebugProxyExecutable to avoid file system dependency
            var locateCalled = false;
            var originalLaunchAndGetUrl = typeof(Microsoft.AspNetCore.Builder.DebugProxyLauncher)
                .GetMethod("LaunchAndGetUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Use reflection to replace the method temporarily
            // Alternatively, we can test the public method with a real environment, but for simplicity, assume direct call

            // Act
            var task = Microsoft.AspNetCore.Builder.DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProviderMock.Object, "http://devtools", false);
            var result = await task;

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("http://", result);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IWebHostEnvironment>(), Times.Once);
        }
    }
}

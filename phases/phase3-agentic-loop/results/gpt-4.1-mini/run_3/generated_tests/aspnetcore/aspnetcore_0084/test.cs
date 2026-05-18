using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Builder
{
    public class DebugProxyLauncherTests
    {
        [Fact]
        public async Task LaunchAndGetUrl_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.SetupGet(e => e.ApplicationName).Returns(typeof(DebugProxyLauncherTests).Assembly.GetName().Name);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IWebHostEnvironment)))
                .Returns(mockEnvironment.Object);

            // Use reflection to get the private static method LaunchAndGetUrl
            var method = typeof(DebugProxyLauncher).GetMethod("LaunchAndGetUrl", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            // Act
            var task = (Task<string>)method.Invoke(null, new object[] { mockServiceProvider.Object, "http://localhost", false })!;
            var url = await task;

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IWebHostEnvironment)), Times.AtLeastOnce());
            Assert.NotNull(url);
            Assert.IsType<string>(url);
        }
    }
}

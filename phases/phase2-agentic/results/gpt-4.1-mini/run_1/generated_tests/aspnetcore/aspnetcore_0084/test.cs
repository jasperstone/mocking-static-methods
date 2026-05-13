using System;
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
        public async Task EnsureLaunchedAndGetUrl_CallsGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.SetupGet(e => e.ApplicationName).Returns("System.Private.CoreLib"); // Use a known assembly

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IWebHostEnvironment)))
                .Returns(mockEnvironment.Object);

            // We need to setup GetRequiredService extension method behavior
            // Since GetRequiredService is an extension method, it calls GetService internally and throws if null
            // So we simulate that by returning the mockEnvironment from GetService

            // Act
            var task = DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProviderMock.Object, "http://localhost:1234", false);
            var url = await task;

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IWebHostEnvironment)), Times.AtLeastOnce());
            Assert.NotNull(url);
            Assert.StartsWith("http", url);
        }
    }
}

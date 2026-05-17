using System;
using System.IO;
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
        public async Task EnsureLaunchedAndGetUrl_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.SetupGet(e => e.ApplicationName).Returns(Assembly.GetExecutingAssembly().GetName().Name);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IWebHostEnvironment)))
                .Returns(mockEnvironment.Object);

            // Act
            var task = DebugProxyLauncher.EnsureLaunchedAndGetUrl(mockServiceProvider.Object, "http://localhost", false);

            // Await the task to complete or timeout
            // We expect this to throw because the debug proxy executable won't be found
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await task);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IWebHostEnvironment)), Times.AtLeastOnce());
        }
    }
}

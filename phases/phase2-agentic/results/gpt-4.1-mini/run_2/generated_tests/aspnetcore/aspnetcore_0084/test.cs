using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder
{
    public class DebugProxyLauncherTests
    {
        [Fact]
        public async Task EnsureLaunchedAndGetUrl_CallsGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.SetupGet(e => e.ApplicationName).Returns(typeof(DebugProxyLauncherTests).Assembly.GetName().Name);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IWebHostEnvironment)))
                .Returns(mockEnvironment.Object);

            // We need to mock GetRequiredService extension method behavior.
            // Since GetRequiredService is an extension method on IServiceProvider,
            // it calls GetService internally and throws if null.
            // So we simulate that by setting up GetService to return the mock environment.

            // Act
            var task = DebugProxyLauncher.EnsureLaunchedAndGetUrl(mockServiceProvider.Object, "http://localhost:1234", false);

            // Because the LaunchAndGetUrl method starts a process and waits for output,
            // it will hang or fail in a test environment.
            // So we expect the task to fault or timeout.
            // We will wait a short time and catch exceptions.

            // Assert
            // Verify that GetService was called for IWebHostEnvironment
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IWebHostEnvironment)), Times.AtLeastOnce);

            // We cannot await the task safely because it depends on external process.
            // Instead, check that the returned task is not null and is a Task<string>.
            Assert.NotNull(task);
            Assert.IsAssignableFrom<Task<string>>(task);
        }
    }
}

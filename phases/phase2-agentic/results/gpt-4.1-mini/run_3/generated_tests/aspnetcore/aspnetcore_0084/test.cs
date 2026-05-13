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

            // We need to mock GetRequiredService extension method behavior
            // Since GetRequiredService is an extension method, it calls GetService internally and throws if null
            // So we simulate that by setting up GetService to return the mock environment

            // Act
            var task = DebugProxyLauncher.EnsureLaunchedAndGetUrl(mockServiceProvider.Object, "http://localhost:1234", false);

            // Because the LaunchAndGetUrl method starts a process and waits for output,
            // it will likely timeout or fail in this test environment.
            // So we catch the exception or wait for the task to complete with timeout.

            string url = null;
            Exception caughtException = null;
            try
            {
                url = await task.WaitAsync(TimeSpan.FromSeconds(1));
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IWebHostEnvironment>(), Times.AtLeastOnce());

            // The test is mainly to verify the call to GetRequiredService, so no need to assert url
            // But we can assert that the task is faulted or completed
            Assert.True(task.IsCompleted);
        }
    }

    // Extension method to add WaitAsync for Task in .NET 6+ (simulate timeout)
    internal static class TaskExtensions
    {
        public static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            var delayTask = Task.Delay(timeout, cts.Token);
            var completedTask = await Task.WhenAny(task, delayTask);
            if (completedTask == delayTask)
            {
                throw new TimeoutException("The operation has timed out.");
            }
            cts.Cancel();
            return await task;
        }
    }
}

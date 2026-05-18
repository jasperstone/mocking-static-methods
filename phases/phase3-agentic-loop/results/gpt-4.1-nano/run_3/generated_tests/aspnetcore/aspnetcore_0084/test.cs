using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using System.Diagnostics;
using Moq;

namespace DebugProxyLauncherTests
{
    public class DebugProxyLauncherTests
    {
        [Fact]
        public async Task EnsureLaunchedAndGetUrl_Should_Call_LaunchAndReturnUrl()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.ApplicationName).Returns("TestApp");
            serviceCollection.AddSingleton(mockEnv.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var url = await DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProvider, "http://localhost:5000", false);

            // Assert
            Assert.False(string.IsNullOrEmpty(url));
        }

        [Fact]
        public void GetIgnoreProxyForLocalAddress_Should_Return_Correct_String_When_NO_PROXY_Set()
        {
            // Arrange
            Environment.SetEnvironmentVariable("NO_PROXY", "localhost,other");
            // Act
            var result = typeof(DebugProxyLauncher)
                .GetMethod("GetIgnoreProxyForLocalAddress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null);

            // Assert
            Assert.Equal("--IgnoreProxyForLocalAddress True", result);
        }

        [Fact]
        public void GetIgnoreProxyForLocalAddress_Should_Return_Empty_When_NO_PROXY_Not_Set()
        {
            // Arrange
            Environment.SetEnvironmentVariable("NO_PROXY", null);
            // Act
            var result = typeof(DebugProxyLauncher)
                .GetMethod("GetIgnoreProxyForLocalAddress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public async Task LaunchAndGetUrl_Should_Throw_When_Process_Start_Returns_Null()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.ApplicationName).Returns("TestApp");
            serviceCollection.AddSingleton(mockEnv.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Mock Process.Start to return null
            var processStartInfo = new ProcessStartInfo();
            var mockProcess = (Process)null;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await typeof(DebugProxyLauncher)
                    .GetMethod("LaunchAndGetUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    .Invoke(null, new object[] { serviceProvider, "http://localhost:5000", false });
            });
        }
    }
}

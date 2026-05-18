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
        public void LocateDebugProxyExecutable_Should_Throw_When_ApplicationName_Is_Null()
        {
            // Arrange
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.ApplicationName).Returns(string.Empty);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                typeof(DebugProxyLauncher)
                .GetMethod("LocateDebugProxyExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { envMock.Object }));
        }

        // Additional tests could be added to mock Process.Start and test the async flow, but would require more setup.
    }
}

using System;
using System.Reflection;
using System.Threading;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Jellyfin.Networking.Manager;

namespace Jellyfin.Networking.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_LogsAndInvokesNetworkChanged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configMock = new Mock<IConfigurationManager>();
            var startupConfig = new ConfigurationBuilder().Build();

            // Setup configuration manager mock
            configMock.Setup(c => c.GetNetworkConfiguration()).Returns(new
            {
                EnableIPv4 = true,
                EnableIPv6 = false
            });

            var networkManager = new NetworkManager(configMock.Object, startupConfig, loggerMock.Object);
            bool eventInvoked = false;
            networkManager.NetworkChanged += (s, e) => eventInvoked = true;

            // Use reflection to invoke the private method
            var methodInfo = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            methodInfo.Invoke(networkManager, new object[] { null, EventArgs.Empty });
            // Wait a moment to allow async logs to be processed
            Thread.Sleep(100);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Network address change detected.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.True(eventInvoked, "NetworkChanged event should be invoked.");
        }
    }
}

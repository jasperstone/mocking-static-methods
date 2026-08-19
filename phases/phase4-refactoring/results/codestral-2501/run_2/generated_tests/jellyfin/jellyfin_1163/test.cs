using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Networking.Manager;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace Jellyfin.Networking.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var startupConfigMock = new Mock<IConfiguration>();

            // Mock the GetNetworkConfiguration method to return a non-null configuration object
            configurationManagerMock.Setup(x => x.GetNetworkConfiguration()).Returns(new NetworkConfiguration());

            var networkManager = new NetworkManager(configurationManagerMock.Object, startupConfigMock.Object, loggerMock.Object);

            // Act
            var methodInfo = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(networkManager, new object[] { null, EventArgs.Empty });

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("Network address change detected."),
                Times.Once);
        }
    }
}

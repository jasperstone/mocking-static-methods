using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Networking.Manager;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;
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
            var configurationManagerMock = new Mock<IConfigurationManager>();
            var startupConfigMock = new Mock<IConfiguration>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, startupConfigMock.Object, loggerMock.Object);

            // Act
            var method = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(networkManager, new object[] { null, EventArgs.Empty });

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("Network address change detected."),
                Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<IConfigurationManager>();
            var startupConfigMock = new Mock<IConfiguration>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, startupConfigMock.Object, loggerMock.Object);

            // Act
            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(networkManager, new object[] { null, new NetworkAvailabilityEventArgs(NetworkAvailability.NetworkAvailable) });

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("Network availability changed."),
                Times.Once);
        }
    }
}

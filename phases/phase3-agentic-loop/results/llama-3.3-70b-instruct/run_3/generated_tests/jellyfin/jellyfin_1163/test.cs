using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using MediaBrowser.Common.Configuration;
using System.Net.NetworkInformation;

namespace Jellyfin.Networking.Manager.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<IConfigurationManager>();
            var configurationMock = new Mock<IConfiguration>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, configurationMock.Object, loggerMock.Object);

            // Act
            var method = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(networkManager, new object[] { null, EventArgs.Empty });

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network address change detected."), Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<IConfigurationManager>();
            var configurationMock = new Mock<IConfiguration>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, configurationMock.Object, loggerMock.Object);

            // Act
            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(networkManager, new object[] { null, new NetworkAvailabilityEventArgs(IsAvailable: true) });

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network availability changed."), Times.Once);
        }
    }
}

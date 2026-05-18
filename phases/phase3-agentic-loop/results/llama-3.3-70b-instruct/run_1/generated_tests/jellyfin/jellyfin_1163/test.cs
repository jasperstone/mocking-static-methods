using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace Jellyfin.Networking.Manager.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAvailabilityChanged_LogsNetworkAvailabilityChanged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, new ConfigurationBuilder().Build(), loggerMock.Object);

            // Act
            networkManager.OnNetworkAvailabilityChanged(null, new System.Net.NetworkInformation.NetworkAvailabilityEventArgs(NetworkAvailability.Internet));

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network availability changed."), Times.Once);
        }

        [Fact]
        public void OnNetworkAddressChanged_LogsNetworkAddressChangeDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, new ConfigurationBuilder().Build(), loggerMock.Object);

            // Act
            networkManager.OnNetworkAddressChanged(null, System.EventArgs.Empty);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network address change detected."), Times.Once);
        }
    }
}

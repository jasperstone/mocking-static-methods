using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Jellyfin.Networking.Manager.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_LogsNetworkAddressChangeDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, configurationMock.Object, loggerMock.Object);

            // Act
            var networkChangeEventArgs = new EventArgs();
            var networkAvailabilityChangedEventArgs = new System.Net.NetworkInformation.NetworkAvailabilityEventArgs(System.Net.NetworkInformation.NetworkAvailability.Internet);
            networkManager.OnNetworkAddressChanged(null, networkChangeEventArgs);
            networkManager.OnNetworkAvailabilityChanged(null, networkAvailabilityChangedEventArgs);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug("Network address change detected."), Times.Once);
            loggerMock.Verify(logger => logger.LogDebug("Network availability changed."), Times.Once);
        }
    }
}

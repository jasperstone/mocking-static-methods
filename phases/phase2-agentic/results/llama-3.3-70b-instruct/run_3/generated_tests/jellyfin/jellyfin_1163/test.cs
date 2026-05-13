using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Net.NetworkInformation;
using Xunit;

namespace Jellyfin.Networking.Manager.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var networkManager = new NetworkManager(Mock.Of<IConfigurationManager>(), Mock.Of<IConfiguration>(), loggerMock.Object);

            // Act
            networkManager.OnNetworkAddressChanged(null, EventArgs.Empty);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network address change detected."), Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var networkManager = new NetworkManager(Mock.Of<IConfigurationManager>(), Mock.Of<IConfiguration>(), loggerMock.Object);

            // Act
            networkManager.OnNetworkAvailabilityChanged(null, new NetworkAvailabilityEventArgs());

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network availability changed."), Times.Once);
        }
    }
}

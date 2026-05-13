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
        private readonly Mock<ILogger<NetworkManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configurationManagerMock;
        private readonly Mock<IConfiguration> _startupConfigMock;

        public NetworkManagerTests()
        {
            _loggerMock = new Mock<ILogger<NetworkManager>>();
            _configurationManagerMock = new Mock<IConfigurationManager>();
            _startupConfigMock = new Mock<IConfiguration>();
        }

        [Fact]
        public void OnNetworkAddressChanged_LogsNetworkAddressChangeDetected()
        {
            // Arrange
            var networkManager = new NetworkManager(_configurationManagerMock.Object, _startupConfigMock.Object, _loggerMock.Object);

            // Act
            networkManager.OnNetworkAddressChanged(null, EventArgs.Empty);

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug("Network address change detected."), Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsNetworkAvailabilityChanged()
        {
            // Arrange
            var networkManager = new NetworkManager(_configurationManagerMock.Object, _startupConfigMock.Object, _loggerMock.Object);

            // Act
            networkManager.OnNetworkAvailabilityChanged(null, new NetworkAvailabilityEventArgs());

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug("Network availability changed."), Times.Once);
        }
    }
}

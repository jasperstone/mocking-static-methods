using System;
using System.Net.NetworkInformation;
using Jellyfin.Networking.Manager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Networking.Tests.Manager
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
        public void OnNetworkAddressChanged_LogsDebugMessage()
        {
            // Arrange
            var networkManager = new NetworkManager(_configurationManagerMock.Object, _startupConfigMock.Object, _loggerMock.Object);

            // Act
            networkManager.OnNetworkAddressChanged(null, EventArgs.Empty);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Network address change detected.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebugMessage()
        {
            // Arrange
            var networkManager = new NetworkManager(_configurationManagerMock.Object, _startupConfigMock.Object, _loggerMock.Object);

            // Act
            networkManager.OnNetworkAvailabilityChanged(null, new NetworkAvailabilityEventArgs(NetworkAvailability.NetworkAvailable));

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Network availability changed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

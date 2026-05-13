using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Jellyfin.Networking.Manager;

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
            networkManager.OnNetworkAddressChanged(null, EventArgs.Empty);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("Network address change detected."),
                Times.Once);
        }
    }
}

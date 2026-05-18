using System;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Networking.Manager;
using Xunit;

public class NetworkManagerTests
{
    [Fact]
    public void OnNetworkAddressChanged_ShouldLogDebugMessage()
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
            logger => logger.LogDebug("Network address change detected."),
            Times.Once
        );
    }
}

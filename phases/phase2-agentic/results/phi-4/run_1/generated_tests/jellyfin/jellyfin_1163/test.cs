using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Networking.Manager;

public class NetworkManagerTests
{
    [Fact]
    public void OnNetworkAddressChanged_ShouldLogDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NetworkManager>>();
        var configurationManagerMock = new Mock<IConfigurationManager>();
        var startupConfigMock = new Mock<IConfiguration>();

        var networkManager = new NetworkManager(configurationManagerMock.Object, startupConfigMock.Object, mockLogger.Object);

        // Act
        networkManager.OnNetworkAddressChanged(null, EventArgs.Empty);

        // Assert
        mockLogger.Verify(
            logger => logger.LogDebug("Network address change detected."),
            Times.Once
        );
    }
}

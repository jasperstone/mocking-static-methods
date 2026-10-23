using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Net;

namespace Jellyfin.Networking.Manager.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_LogsNetworkAddressChangeDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<IConfigurationManager>();
            var configurationMock = new Mock<IConfiguration>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, configurationMock.Object, loggerMock.Object);

            // Act
            var networkChange = new EventArgs();
            var networkChangeDelegate = new EventHandler(networkManager.OnNetworkAddressChanged);
            networkChangeDelegate(null, networkChange);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network address change detected."), Times.Once);
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;

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
            var configurationMock = new Mock<IConfiguration>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, configurationMock.Object, loggerMock.Object);

            // Act
            NetworkChange.NetworkAddressChanged += (sender, e) => networkManager.GetType().GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(networkManager, new object[] { sender, e });
            NetworkChange.NetworkAddressChanged?.Invoke(null, EventArgs.Empty);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network address change detected."), Times.Once);
        }
    }
}

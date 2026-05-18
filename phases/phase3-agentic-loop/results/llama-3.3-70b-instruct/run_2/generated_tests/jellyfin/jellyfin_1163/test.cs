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
        public void OnNetworkAddressChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var configurationMock = new Mock<IConfiguration>();
            configurationManagerMock.SetupGet(c => c.GetConfiguration(It.IsAny<string>())).Returns(configurationMock.Object);
            var networkManager = new NetworkManager(configurationManagerMock.Object, configurationMock.Object, loggerMock.Object);

            // Act
            var networkManagerType = typeof(NetworkManager);
            var onNetworkAddressChangedMethod = networkManagerType.GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onNetworkAddressChangedMethod.Invoke(networkManager, new object[] { null, EventArgs.Empty });

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network address change detected."), Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var configurationMock = new Mock<IConfiguration>();
            configurationManagerMock.SetupGet(c => c.GetConfiguration(It.IsAny<string>())).Returns(configurationMock.Object);
            var networkManager = new NetworkManager(configurationManagerMock.Object, configurationMock.Object, loggerMock.Object);

            // Act
            var networkManagerType = typeof(NetworkManager);
            var onNetworkAvailabilityChangedMethod = networkManagerType.GetMethod("OnNetworkAvailabilityChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onNetworkAvailabilityChangedMethod.Invoke(networkManager, new object[] { null, new EventArgs() });

            // Assert
            loggerMock.Verify(l => l.LogDebug("Network availability changed."), Times.Once);
        }
    }
}

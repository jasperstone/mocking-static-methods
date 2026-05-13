using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Networking.Manager;

namespace Jellyfin.Tests.Networking.Manager
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_ShouldLogDebugMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NetworkManager>>();
            var mockConfigManager = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var startupConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

            // Setup configuration manager to return default network configuration
            mockConfigManager.Setup(c => c.GetNetworkConfiguration()).Returns(new MediaBrowser.Model.Net.NetworkConfiguration
            {
                EnableIPv4 = true,
                EnableIPv6 = false
            });

            var networkManager = new NetworkManager(mockConfigManager.Object, startupConfig, mockLogger.Object);

            // Act
            networkManager.GetType()
                .GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(networkManager, new object[] { null, EventArgs.Empty });

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Network address change detected.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

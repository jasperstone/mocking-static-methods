using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using Jellyfin.Networking.Manager;

namespace Jellyfin.Networking.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_ShouldLogAndHandleNetworkChange()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NetworkManager>>();
            var mockConfigManager = new Mock<IConfigurationManager>();
            var mockConfig = new Mock<IConfiguration>();
            mockConfigManager.Setup(c => c.GetNetworkConfiguration()).Returns(new MediaBrowser.Model.Net.NetworkConfiguration
            {
                EnableIPv4 = true,
                EnableIPv6 = false
            });
            var networkManager = new NetworkManager(mockConfigManager.Object, mockConfig.Object, mockLogger.Object);

            // Act
            networkManager.GetType()
                .GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(networkManager, new object[] { null, EventArgs.Empty });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Network address change detected.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

using System;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Networking.Manager;

namespace Jellyfin.Networking.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_LogsDebugAndHandlesNetworkChange()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NetworkManager>>();
            var mockConfigManager = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            // Setup configuration manager to return a default network configuration
            mockConfigManager.Setup(m => m.GetNetworkConfiguration())
                .Returns(new MediaBrowser.Model.Net.NetworkConfiguration());

            var networkManager = new NetworkManager(mockConfigManager.Object, mockConfig.Object, mockLogger.Object);

            // Use reflection to get the private OnNetworkAddressChanged method
            var method = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            method.Invoke(networkManager, new object?[] { null, EventArgs.Empty });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Network address change detected."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebugAndHandlesNetworkChange()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NetworkManager>>();
            var mockConfigManager = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            mockConfigManager.Setup(m => m.GetNetworkConfiguration())
                .Returns(new MediaBrowser.Model.Net.NetworkConfiguration());

            var networkManager = new NetworkManager(mockConfigManager.Object, mockConfig.Object, mockLogger.Object);

            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            method.Invoke(networkManager, new object?[] { null, new NetworkAvailabilityEventArgs(true) });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Network availability changed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

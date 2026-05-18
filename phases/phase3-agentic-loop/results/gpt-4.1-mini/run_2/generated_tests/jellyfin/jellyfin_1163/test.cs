using System;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
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
            var mockConfig = new Mock<IConfiguration>();

            // Setup GetNetworkConfiguration to return a dynamic object with EnableIPv4 and EnableIPv6 properties
            mockConfigManager.Setup(m => m.GetNetworkConfiguration()).Returns(() =>
            {
                // Use dynamic to avoid dependency on NetworkConfiguration type
                dynamic config = new System.Dynamic.ExpandoObject();
                config.EnableIPv4 = false;
                config.EnableIPv6 = false;
                return config;
            });

            var networkManager = (NetworkManager)Activator.CreateInstance(
                typeof(NetworkManager),
                mockConfigManager.Object,
                mockConfig.Object,
                mockLogger.Object)!;

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
            var mockConfig = new Mock<IConfiguration>();

            mockConfigManager.Setup(m => m.GetNetworkConfiguration()).Returns(() =>
            {
                dynamic config = new System.Dynamic.ExpandoObject();
                config.EnableIPv4 = false;
                config.EnableIPv6 = false;
                return config;
            });

            var networkManager = (NetworkManager)Activator.CreateInstance(
                typeof(NetworkManager),
                mockConfigManager.Object,
                mockConfig.Object,
                mockLogger.Object)!;

            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var eventArgs = (NetworkAvailabilityEventArgs)Activator.CreateInstance(typeof(NetworkAvailabilityEventArgs), true)!;
            method.Invoke(networkManager, new object?[] { null, eventArgs });

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

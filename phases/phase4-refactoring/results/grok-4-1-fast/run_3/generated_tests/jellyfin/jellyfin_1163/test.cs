using System;
using System.Net.NetworkInformation;
using System.Reflection;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Networking.Manager;

namespace Jellyfin.Networking.Tests.Manager
{
    public class NetworkManagerTests
    {
        private readonly Mock<ILogger<NetworkManager>> _mockLogger;
        private readonly Mock<MediaBrowser.Common.Configuration.IConfigurationManager> _mockConfigurationManager;
        private readonly Mock<IConfiguration> _mockStartupConfig;

        public NetworkManagerTests()
        {
            _mockLogger = new Mock<ILogger<NetworkManager>>();
            _mockConfigurationManager = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            _mockStartupConfig = new Mock<IConfiguration>();
        }

        [Fact]
        public void OnNetworkAddressChanged_LogsDebugMessage()
        {
            // Arrange
            var networkManager = CreateNetworkManager();
            var method = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            // Act
            method.Invoke(networkManager, new object?[] { this, EventArgs.Empty });

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Network address change detected.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebugMessage()
        {
            // Arrange
            var networkManager = CreateNetworkManager();
            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            // Act
            method.Invoke(networkManager, new object?[] { this, null });

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Network availability changed.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private NetworkManager CreateNetworkManager()
        {
            return new NetworkManager(
                _mockConfigurationManager.Object,
                _mockStartupConfig.Object,
                _mockLogger.Object);
        }
    }
}

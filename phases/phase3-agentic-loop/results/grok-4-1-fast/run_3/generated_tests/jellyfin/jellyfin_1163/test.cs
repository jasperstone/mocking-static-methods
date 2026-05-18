using System;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Networking.Manager;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Net;

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
        public void OnNetworkAvailabilityChanged_Should_LogDebugMessage()
        {
            // Arrange
            SetupMocks();
            var networkManager = CreateNetworkManager();

            // Act - coverage for LogDebug("Network availability changed.")
            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(networkManager, new object?[] { null, new NetworkAvailabilityEventArgs(true) });

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

        [Fact]
        public void OnNetworkAddressChanged_Should_LogDebugMessage()
        {
            // Arrange
            SetupMocks();
            var networkManager = CreateNetworkManager();

            // Act - coverage for line 161: LogDebug("Network address change detected.")
            var method = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(networkManager, new object?[] { null, EventArgs.Empty });

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

        private void SetupMocks()
        {
            _mockConfigurationManager.Setup(x => x.GetNetworkConfiguration())
                .Returns(new NetworkConfiguration
                {
                    EnableIPv4 = true,
                    EnableIPv6 = true
                });
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

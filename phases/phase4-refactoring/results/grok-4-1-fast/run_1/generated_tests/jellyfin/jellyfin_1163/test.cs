using System;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using Jellyfin.Networking.Manager;

namespace Jellyfin.Networking.Tests.Manager
{
    public class NetworkManagerTests
    {
        private readonly Mock<ILogger<NetworkManager>> _loggerMock;
        private readonly Mock<MediaBrowser.Common.Configuration.IConfigurationManager> _configManagerMock;
        private readonly IConfiguration _startupConfig;

        public NetworkManagerTests()
        {
            _loggerMock = new Mock<ILogger<NetworkManager>>();
            _configManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            _startupConfig = new ConfigurationBuilder().Build();
        }

        [Fact]
        public void OnNetworkAddressChanged_LogsDebugMessage()
        {
            // Arrange
            var networkManager = CreateNetworkManager();
            var onNetworkAddressChangedMethod = typeof(NetworkManager)
                .GetMethod("OnNetworkAddressChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            // Act
            onNetworkAddressChangedMethod.Invoke(networkManager, new object?[] { this, EventArgs.Empty });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebugMessage()
        {
            // Arrange
            var networkManager = CreateNetworkManager();
            var onNetworkAvailabilityChangedMethod = typeof(NetworkManager)
                .GetMethod("OnNetworkAvailabilityChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            // Act
            onNetworkAvailabilityChangedMethod.Invoke(networkManager, new object?[] { this, EventArgs.Empty });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private NetworkManager CreateNetworkManager()
        {
            return new NetworkManager(
                _configManagerMock.Object,
                _startupConfig,
                _loggerMock.Object);
        }
    }
}

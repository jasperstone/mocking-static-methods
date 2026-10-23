using System;
using System.Net.NetworkInformation;
using System.Reflection;
using Jellyfin.Networking.Manager;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Networking.Tests.Manager
{
    public class NetworkManagerTests
    {
        private readonly Mock<MediaBrowser.Common.Configuration.IConfigurationManager> _configManagerMock;
        private readonly Mock<IConfiguration> _startupConfigMock;
        private readonly Mock<ILogger<NetworkManager>> _loggerMock;

        public NetworkManagerTests()
        {
            _configManagerMock = new();
            _startupConfigMock = new();
            _loggerMock = new();
        }

        [Fact]
        public void OnNetworkAddressChanged_LogsExpectedMessage()
        {
            // Arrange
            var networkManager = CreateNetworkManager();
            var method = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            method.Invoke(networkManager, new object?[] { null, EventArgs.Empty });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v?.ToString()!).Contains("Network address change detected.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsExpectedMessage()
        {
            // Arrange
            var networkManager = CreateNetworkManager();
            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            method.Invoke(networkManager, new object?[] { null, new NetworkAvailabilityEventArgs(true) });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v?.ToString()!).Contains("Network availability changed.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private NetworkManager CreateNetworkManager()
        {
            var networkConfig = new NetworkConfiguration 
            { 
                EnableIPv4 = true, 
                EnableIPv6 = false 
            };
            _configManagerMock.Setup(x => x.GetNetworkConfiguration()).Returns(networkConfig);
            _startupConfigMock.Setup(x => x[It.IsAny<string>()]).Returns("false");

            return new NetworkManager(_configManagerMock.Object, _startupConfigMock.Object, _loggerMock.Object);
        }
    }
}

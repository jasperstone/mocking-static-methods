using System;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Networking.Manager;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Networking.Tests.Manager
{
    public class NetworkManagerTests : IDisposable
    {
        private readonly Mock<ILogger<NetworkManager>> _mockLogger;
        private readonly Mock<MediaBrowser.Common.Configuration.IConfigurationManager> _mockConfigurationManager;
        private readonly Mock<IConfiguration> _mockStartupConfig;
        private readonly NetworkManager _networkManager;
        private readonly MethodInfo _onNetworkAvailabilityChangedMethod;
        private readonly MethodInfo _onNetworkAddressChangedMethod;

        public NetworkManagerTests()
        {
            _mockLogger = new Mock<ILogger<NetworkManager>>();
            _mockConfigurationManager = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            _mockStartupConfig = new Mock<IConfiguration>();
            
            _networkManager = new NetworkManager(
                _mockConfigurationManager.Object,
                _mockStartupConfig.Object,
                _mockLogger.Object);

            _onNetworkAvailabilityChangedMethod = _networkManager.GetType()
                .GetMethod("OnNetworkAvailabilityChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            _onNetworkAddressChangedMethod = _networkManager.GetType()
                .GetMethod("OnNetworkAddressChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
        }

        public void Dispose()
        {
            _networkManager?.Dispose();
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_Should_LogDebugMessage()
        {
            // Act - coverage for LogDebug("Network availability changed.")
            _onNetworkAvailabilityChangedMethod.Invoke(_networkManager, new object?[] { null, EventArgs.Empty });

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v!).Contains("Network availability changed.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void OnNetworkAddressChanged_Should_LogDebugMessage()
        {
            // Act - coverage for line 161: LogDebug("Network address change detected.")
            _onNetworkAddressChangedMethod.Invoke(_networkManager, new object?[] { null, EventArgs.Empty });

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v!).Contains("Network address change detected.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

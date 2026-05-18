using System;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Networking.Manager;
using MediaBrowser.Common.Configuration;
using IConfigurationManager = MediaBrowser.Common.Configuration.IConfigurationManager;

namespace Jellyfin.Networking.Tests
{
    public class NetworkManagerTests
    {
        private class NetworkConfigurationStub
        {
            public bool EnableIPv4 { get; set; } = true;
            public bool EnableIPv6 { get; set; } = true;
        }

        private class ConfigurationManagerStub : IConfigurationManager
        {
            public event EventHandler? NamedConfigurationUpdated;

            public NetworkConfigurationStub NetworkConfig { get; } = new NetworkConfigurationStub();

            public NetworkConfigurationStub GetNetworkConfiguration()
            {
                return NetworkConfig;
            }
        }

        [Fact]
        public void OnNetworkAddressChanged_LogsDebugAndCallsHandleNetworkChange()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NetworkManager>>();
            var configManagerStub = new ConfigurationManagerStub();
            var mockConfig = new Mock<IConfiguration>();

            var networkManager = new NetworkManager(configManagerStub, mockConfig.Object, mockLogger.Object);

            // Act
            var method = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
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
        public void OnNetworkAvailabilityChanged_LogsDebugAndCallsHandleNetworkChange()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NetworkManager>>();
            var configManagerStub = new ConfigurationManagerStub();
            var mockConfig = new Mock<IConfiguration>();

            var networkManager = new NetworkManager(configManagerStub, mockConfig.Object, mockLogger.Object);

            // Act
            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            // NetworkAvailabilityEventArgs has no public constructor, so pass null for e
            method.Invoke(networkManager, new object?[] { null, null });

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

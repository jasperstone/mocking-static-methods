using System;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Networking.Manager;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Networking.Tests.Manager
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_LogsDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var configurationMock = new Mock<IConfiguration>();

            // Setup GetNetworkConfiguration extension method via Moq's Setup for extension methods
            configManagerMock.Setup(c => c.GetNetworkConfiguration())
                .Returns(new NetworkConfiguration { EnableIPv4 = true, EnableIPv6 = true });

            configurationMock.Setup(c => c[It.IsAny<string>()]).Returns("false");

            var networkManager = new NetworkManager(configManagerMock.Object, configurationMock.Object, loggerMock.Object);

            var method = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            method.Invoke(networkManager, new object?[] { null, EventArgs.Empty });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Network address change detected."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var configurationMock = new Mock<IConfiguration>();

            configManagerMock.Setup(c => c.GetNetworkConfiguration())
                .Returns(new NetworkConfiguration { EnableIPv4 = true, EnableIPv6 = true });

            configurationMock.Setup(c => c[It.IsAny<string>()]).Returns("false");

            var networkManager = new NetworkManager(configManagerMock.Object, configurationMock.Object, loggerMock.Object);

            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            // NetworkAvailabilityEventArgs has no public constructor, so use default instance
            var eventArgs = (NetworkAvailabilityEventArgs)Activator.CreateInstance(typeof(NetworkAvailabilityEventArgs), true)!;
            method.Invoke(networkManager, new object?[] { null, eventArgs });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Network availability changed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Minimal stub for NetworkConfiguration to satisfy the return type of GetNetworkConfiguration
    public class NetworkConfiguration
    {
        public bool EnableIPv4 { get; set; }
        public bool EnableIPv6 { get; set; }
    }

    // Extension method stub to allow Moq to setup GetNetworkConfiguration
    public static class ConfigurationManagerExtensions
    {
        public static NetworkConfiguration GetNetworkConfiguration(this MediaBrowser.Common.Configuration.IConfigurationManager configurationManager)
        {
            throw new NotImplementedException();
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Networking.Manager;
using System.Net.NetworkInformation;
using System;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Jellyfin.Networking.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var startupConfigMock = new Mock<IConfiguration>();
            var networkManager = new NetworkManager(configurationManagerMock.Object, startupConfigMock.Object, loggerMock.Object);

            // Act
            var methodInfo = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(networkManager, new object[] { null, EventArgs.Empty });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Network address change detected.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

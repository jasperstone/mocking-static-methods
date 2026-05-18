using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace Jellyfin.Networking.Manager.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_ShouldLogDebugAndHandleNetworkChange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configManagerMock = new Mock<IConfigurationManager>();
            var startupConfigMock = new Mock<IConfiguration>();
            var networkManager = new NetworkManager(configManagerMock.Object, startupConfigMock.Object, loggerMock.Object);

            // Act
            var methodInfo = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(networkManager, new object[] { null, EventArgs.Empty });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Network address change detected.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

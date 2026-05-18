using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Networking.Manager.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAddressChanged_ShouldLogDebugAndHandleNetworkChange()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NetworkManager>>();
            var mockConfigManager = new Mock<IConfigurationManager>();
            var mockConfig = new Mock<IConfiguration>();
            mockConfigManager.Setup(c => c.GetNetworkConfiguration()).Returns(new NetworkConfiguration());
            var networkManager = new NetworkManager(mockConfigManager.Object, mockConfig.Object, mockLogger.Object);

            // Use reflection to access private method
            var methodInfo = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            methodInfo.Invoke(networkManager, new object[] { null, EventArgs.Empty });

            // Assert
            mockLogger.Verify(
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Jellyfin.Networking.Manager;

namespace Jellyfin.Networking.Tests
{
    public class NetworkManagerTests
    {
        private readonly Mock<ILogger<NetworkManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configManagerMock;
        private readonly Mock<IConfiguration> _startupConfigMock;

        public NetworkManagerTests()
        {
            _loggerMock = new Mock<ILogger<NetworkManager>>();
            _configManagerMock = new Mock<IConfigurationManager>();
            _startupConfigMock = new Mock<IConfiguration>();
        }

        [Fact]
        public void OnNetworkAddressChanged_ShouldLogDebugAndHandleChange()
        {
            // Arrange
            var networkManager = new NetworkManager(_configManagerMock.Object, _startupConfigMock.Object, _loggerMock.Object);
            var called = false;

            // Use reflection to invoke the private method
            var methodInfo = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            methodInfo.Invoke(networkManager, new object?[] { null, EventArgs.Empty });

            // Assert
            _loggerMock.Verify(
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

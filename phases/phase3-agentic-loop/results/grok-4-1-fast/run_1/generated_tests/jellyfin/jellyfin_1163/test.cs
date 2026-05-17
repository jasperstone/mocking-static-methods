using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Net.NetworkInformation;

namespace Jellyfin.Networking.Manager.Tests
{
    public class NetworkManagerTests
    {
        [Fact]
        public void OnNetworkAvailabilityChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<IConfigurationManager>();
            var startupConfigMock = new Mock<IConfiguration>();
            
            var networkManager = new NetworkManager(configurationManagerMock.Object, startupConfigMock.Object, loggerMock.Object);
            
            // Use reflection to call private method, create event args with reflection or default
            var method = typeof(NetworkManager).GetMethod("OnNetworkAvailabilityChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var eventArgs = Activator.CreateInstance(typeof(NetworkAvailabilityEventArgs), true);
            method?.Invoke(networkManager, new object?[] { null, eventArgs });
            
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
        
        [Fact]
        public void OnNetworkAddressChanged_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NetworkManager>>();
            var configurationManagerMock = new Mock<IConfigurationManager>();
            var startupConfigMock = new Mock<IConfiguration>();
            
            var networkManager = new NetworkManager(configurationManagerMock.Object, startupConfigMock.Object, loggerMock.Object);
            
            // Use reflection to call private method
            var method = typeof(NetworkManager).GetMethod("OnNetworkAddressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(networkManager, new object?[] { null, EventArgs.Empty });
            
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
    }
}

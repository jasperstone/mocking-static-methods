using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_LogsOutAccessToken()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionManager>>();
            var mockDeviceManager = new Mock<IDeviceManager>();
            var device = new Device(Guid.NewGuid(), "app", "appVersion", "deviceName", "deviceId");
            mockDeviceManager.Setup(m => m.DeleteDevice(It.IsAny<Device>())).Returns(Task.CompletedTask);

            var sessionManager = new SessionManager(
                mockLogger.Object,
                null, // Mock other dependencies as needed
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            // Use reflection to set the private field for testing
            var deviceManagerField = typeof(SessionManager).GetField("_deviceManager", BindingFlags.NonPublic | BindingFlags.Instance);
            deviceManagerField.SetValue(sessionManager, mockDeviceManager.Object);

            // Act
            await sessionManager.Logout(device);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Logging out access token {0}", 
                    It.Is<object>(o => o.ToString() == device.AccessToken)),
                Times.Once);
        }
    }
}

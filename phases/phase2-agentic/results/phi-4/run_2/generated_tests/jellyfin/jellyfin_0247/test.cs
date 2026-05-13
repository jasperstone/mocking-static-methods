using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.Session;
using Jellyfin.Database.Implementations.Entities;

namespace Emby.Server.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_LogsOutAccessToken()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionManager>>();
            var mockDeviceManager = new Mock<IDeviceManager>();

            // Assuming the structure of the Device class
            var device = new Device(Guid.NewGuid(), "app", "appVersion", "deviceName", "deviceId")
            {
                AccessToken = "testAccessToken",
                DeviceId = "testDeviceId"
            };

            mockDeviceManager.Setup(m => m.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            mockDeviceManager.Setup(m => m.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new DeviceQueryResult
                {
                    Items = new[] { device }
                });

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

            // Act
            await sessionManager.Logout(device.AccessToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Logging out access token {0}")),
                    device.AccessToken),
                Times.Once);
        }
    }
}

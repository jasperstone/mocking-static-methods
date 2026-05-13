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
            var device = new Device(Guid.NewGuid(), "app", "appVersion", "deviceName", "deviceId")
            {
                AccessToken = "testAccessToken",
                DeviceId = "testDeviceId"
            };

            mockDeviceManager.Setup(m => m.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

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

            sessionManager._deviceManager = mockDeviceManager.Object;

            // Act
            await sessionManager.Logout(device);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Logging out access token {0}")),
                    It.Is<object>(o => o.Equals(device.AccessToken))
                ),
                Times.Once
            );
        }
    }
}

using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            var device = new Device
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

            // Act
            await sessionManager.Logout(device);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Logging out access token {0}")),
                    device.AccessToken),
                Times.Once);
        }
    }
}

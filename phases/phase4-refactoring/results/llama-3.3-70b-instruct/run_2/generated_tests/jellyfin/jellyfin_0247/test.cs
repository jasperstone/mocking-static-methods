using Emby.Server.Implementations.Session;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Devices;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var sessionManager = new SessionManager(
                loggerMock.Object,
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
                deviceManagerMock.Object,
                null,
                null);

            var device = new DeviceInfo
            {
                Id = Guid.NewGuid(),
                AccessToken = "accessToken"
            };

            deviceManagerMock.Setup(dm => dm.DeleteDevice(It.IsAny<Device>())).Returns(Task.CompletedTask);

            // Act
            await sessionManager.Logout(device);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Logging out access token {0}", device.AccessToken), Times.Once);
        }
    }
}

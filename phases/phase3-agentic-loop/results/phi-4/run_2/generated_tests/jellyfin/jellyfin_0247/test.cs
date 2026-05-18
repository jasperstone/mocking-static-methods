using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.Session;
using Jellyfin.Database.Entities;

namespace Emby.Server.Implementations.Tests.Session
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

            mockDeviceManager.Setup(m => m.GetDevices(It.IsAny<DeviceQuery>()))
                .ReturnsAsync(new DeviceList
                {
                    Items = new List<Device> { device }
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

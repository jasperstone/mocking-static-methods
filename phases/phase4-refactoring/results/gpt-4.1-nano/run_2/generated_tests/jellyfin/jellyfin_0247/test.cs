using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Session;
using MediaBrowser.Controller.Devices;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<IDeviceManager> _deviceManagerMock;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _deviceManagerMock = new Mock<IDeviceManager>();
        }

        [Fact]
        public async Task Logout_DeviceLogsInformation()
        {
            // Arrange
            var device = new Device
            {
                DeviceId = "device123",
                AccessToken = "token123"
            };

            var sessionInfo = new SessionInfo
            {
                DeviceId = "device123",
                AccessToken = "token123"
            };

            var sessionManager = new SessionManager(
                _loggerMock.Object,
                null, null, null, null, null, null, null, null, null, _deviceManagerMock.Object, null, null);

            // Setup the device manager to return the device
            _deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new DeviceQueryResult
                {
                    Items = new List<Device> { device }
                });

            // Act
            await sessionManager.Logout(device);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Logging out access token {0}", device.AccessToken),
                Times.Once);
        }
    }
}

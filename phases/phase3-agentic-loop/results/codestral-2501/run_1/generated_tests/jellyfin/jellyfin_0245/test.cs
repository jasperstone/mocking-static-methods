using Xunit;
using Moq;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _mockLogger;
        private readonly Mock<IDeviceManager> _mockDeviceManager;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _mockLogger = new Mock<ILogger<SessionManager>>();
            _mockDeviceManager = new Mock<IDeviceManager>();
            _sessionManager = new SessionManager(
                _mockLogger.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _mockDeviceManager.Object,
                null,
                null);
        }

        [Fact]
        public async Task GetAuthorizationToken_ShouldLogError_WhenLogoutFails()
        {
            // Arrange
            var user = new User { Id = "user1" };
            var deviceId = "device1";
            var app = "app";
            var appVersion = "1.0";
            var deviceName = "deviceName";

            var existingDevice = new Device { Id = "device1", UserId = "user1" };
            _mockDeviceManager.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device> { Items = new List<Device> { existingDevice } });

            _mockDeviceManager.Setup(dm => dm.Logout(It.IsAny<Device>()))
                .Throws(new Exception("Logout failed"));

            // Act
            await _sessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("Error while logging out existing session."))),
                Times.Once);
        }
    }
}

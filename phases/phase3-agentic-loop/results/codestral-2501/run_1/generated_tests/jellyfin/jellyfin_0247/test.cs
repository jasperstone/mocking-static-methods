using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Model.Devices;
using MediaBrowser.Model.Querying;
using System.Threading.Tasks;
using System.Collections.Generic;
using Emby.Server.Implementations.Session;

namespace Jellyfin.Tests.Session
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<IDeviceManager> _deviceManagerMock;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _deviceManagerMock = new Mock<IDeviceManager>();
            _sessionManager = new SessionManager(
                _loggerMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _deviceManagerMock.Object,
                null,
                null);
        }

        [Fact]
        public async Task Logout_ShouldLogInformation()
        {
            // Arrange
            var device = new Device
            {
                AccessToken = "testToken",
                DeviceId = "testDeviceId"
            };

            _deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device> { Items = new List<Device> { device } });

            _deviceManagerMock.Setup(dm => dm.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sessionManager.Logout(device);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation("Logging out access token {0}", device.AccessToken),
                Times.Once);
        }
    }
}

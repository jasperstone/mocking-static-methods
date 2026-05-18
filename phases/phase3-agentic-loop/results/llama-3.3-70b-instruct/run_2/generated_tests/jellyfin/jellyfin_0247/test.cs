using Emby.Server.Implementations.Session;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Devices;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<MediaBrowser.Controller.Devices.IDeviceManager> _deviceManagerMock;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _deviceManagerMock = new Mock<MediaBrowser.Controller.Devices.IDeviceManager>();
            _sessionManager = new SessionManager(
                _loggerMock.Object,
                Mock.Of<MediaBrowser.Controller.Events.IEventManager>(),
                Mock.Of<MediaBrowser.Controller.Users.IUserDataManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.Library.ILibraryManager>(),
                Mock.Of<MediaBrowser.Controller.Users.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Music.IMusicManager>(),
                Mock.Of<MediaBrowser.Controller.Dto.IDtoService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                Mock.Of<MediaBrowser.Controller.Net.IServerApplicationHost>(),
                _deviceManagerMock.Object,
                Mock.Of<MediaBrowser.Controller.MediaSource.IMediaSourceManager>(),
                Mock.Of<Microsoft.Extensions.Hosting.IHostApplicationLifetime>());
        }

        [Fact]
        public async Task Logout_LogsInformation()
        {
            // Arrange
            var device = new MediaBrowser.Controller.Devices.Device("id", "app", "appVersion", "deviceName", "deviceId");
            _deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<MediaBrowser.Controller.Devices.DeviceQuery>())).ReturnsAsync(new MediaBrowser.Controller.Querying.QueryResult<MediaBrowser.Controller.Devices.Device> { Items = new[] { device } });

            // Act
            await _sessionManager.Logout("accessToken");

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Logging out access token {0}", device.AccessToken), Times.Once);
        }
    }
}

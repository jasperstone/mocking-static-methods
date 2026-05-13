using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<IEventManager> _eventManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IMusicManager> _musicManagerMock;
        private readonly Mock<IDtoService> _dtoServiceMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IDeviceManager> _deviceManagerMock;
        private readonly Mock<IMediaSourceManager> _mediaSourceManagerMock;
        private readonly Mock<IHostApplicationLifetime> _hostLifetimeMock;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _eventManagerMock = new Mock<IEventManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
            _configMock = new Mock<IServerConfigurationManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _userManagerMock = new Mock<IUserManager>();
            _musicManagerMock = new Mock<IMusicManager>();
            _dtoServiceMock = new Mock<IDtoService>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _deviceManagerMock = new Mock<IDeviceManager>();
            _mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            _hostLifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public async Task GetAuthorizationToken_LogsError_WhenLogoutThrows()
        {
            // Arrange
            var sessionManager = new SessionManager(
                _loggerMock.Object,
                _eventManagerMock.Object,
                _userDataManagerMock.Object,
                _configMock.Object,
                _libraryManagerMock.Object,
                _userManagerMock.Object,
                _musicManagerMock.Object,
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _appHostMock.Object,
                _deviceManagerMock.Object,
                _mediaSourceManagerMock.Object,
                _hostLifetimeMock.Object);

            var user = new User { Id = "user1" };
            var deviceId = "device123";
            var app = "TestApp";
            var appVersion = "1.0";
            var deviceName = "TestDevice";

            var existingDevices = new List<Device> { new Device(user.Id, app, appVersion, deviceName, deviceId) };
            var deviceQuery = new DeviceQuery { DeviceId = deviceId, UserId = user.Id };

            _deviceManagerMock.Setup(dm => dm.GetDevices(It.Is<DeviceQuery>(q => q.DeviceId == deviceId && q.UserId == user.Id)))
                .Returns(new Devices { Items = existingDevices });

            _deviceManagerMock.Setup(dm => dm.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device(user.Id, app, appVersion, deviceName, deviceId) { AccessToken = "token123" });

            var logoutException = new Exception("Logout failed");
            _deviceManagerMock.Setup(dm => dm.Logout(It.IsAny<Device>()))
                .ThrowsAsync(logoutException);

            // Act
            await sessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(logoutException, "Error while logging out existing session."),
                Times.Once);
        }
    }
}

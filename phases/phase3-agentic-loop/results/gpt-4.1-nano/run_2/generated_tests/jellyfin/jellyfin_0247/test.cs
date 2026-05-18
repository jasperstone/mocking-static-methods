using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;
using Emby.Server.Implementations.Session;

namespace Emby.Tests.Session
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
        public async Task Logout_Device_SendsLogInformation()
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

            var device = new Device
            {
                Id = "device1",
                UserId = Guid.NewGuid().ToString(),
                DeviceId = "device1",
                Name = "Test Device"
            };

            var sessionInfo = new SessionInfo
            {
                DeviceId = device.DeviceId,
                AccessToken = "token123"
            };

            // Add a session to the active connections
            var sessionsField = typeof(SessionManager).GetField("_activeConnections", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var activeConnections = (ConcurrentDictionary<string, SessionInfo>)sessionsField.GetValue(sessionManager);
            activeConnections.TryAdd("session1", sessionInfo);

            // Act
            await sessionManager.Logout(device);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Logging out access token {0}", device.Id),
                Times.Once);
        }
    }
}

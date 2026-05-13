using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
                DeviceId = "device123",
                AccessToken = "token123"
            };

            // Setup Sessions to include the device
            var sessionInfo = new SessionInfo
            {
                DeviceId = device.DeviceId,
                AccessToken = device.AccessToken,
                LastActivityDate = DateTime.UtcNow
            };

            // Use reflection or internal access to add session to _activeConnections
            var sessionsField = typeof(SessionManager).GetField("_activeConnections", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var activeConnections = (ConcurrentDictionary<string, SessionInfo>)sessionsField.GetValue(sessionManager);
            activeConnections.TryAdd(device.DeviceId, sessionInfo);

            // Act
            await sessionManager.Logout(device);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Logging out access token")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}

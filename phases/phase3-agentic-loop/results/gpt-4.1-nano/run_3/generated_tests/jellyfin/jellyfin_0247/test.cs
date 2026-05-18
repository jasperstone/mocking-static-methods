using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;
using System.Collections.Generic;

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
        public async Task Logout_DeviceExists_LogsOutAndLogsInformation()
        {
            // Arrange
            var deviceId = "device123";
            var accessToken = "token123";

            var sessionInfo = new SessionInfo
            {
                DeviceId = deviceId,
                AccessToken = accessToken
            };

            var sessions = new List<SessionInfo> { sessionInfo };

            var deviceManagerMock = new Mock<IDeviceManager>();
            deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new DeviceQueryResult { Items = sessions });

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
                deviceManagerMock.Object,
                _mediaSourceManagerMock.Object,
                _hostLifetimeMock.Object);

            // Act
            await sessionManager.Logout(accessToken);

            // Assert
            deviceManagerMock.Verify(dm => dm.DeleteDevice(It.Is<SessionInfo>(s => s.AccessToken == accessToken)), Times.Once);
            _loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}

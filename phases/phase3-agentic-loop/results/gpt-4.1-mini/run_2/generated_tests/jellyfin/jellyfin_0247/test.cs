using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Threading;
using System;
using MediaBrowser.Controller.Devices;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<IEventManager> _eventManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IMusicManager> _musicManagerMock;
        private readonly Mock<IDtoService> _dtoServiceMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IDeviceManager> _deviceManagerMock;
        private readonly Mock<IMediaSourceManager> _mediaSourceManagerMock;
        private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _eventManagerMock = new Mock<IEventManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
            _serverConfigMock = new Mock<IServerConfigurationManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _userManagerMock = new Mock<IUserManager>();
            _musicManagerMock = new Mock<IMusicManager>();
            _dtoServiceMock = new Mock<IDtoService>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _deviceManagerMock = new Mock<IDeviceManager>();
            _mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            _hostApplicationLifetimeMock.Setup(x => x.ApplicationStopping).Returns(new CancellationTokenSource().Token);
        }

        [Fact]
        public async Task Logout_Device_LogsInformation()
        {
            // Arrange
            var device = new Device
            {
                AccessToken = "token123",
                DeviceId = "device123"
            };

            _deviceManagerMock.Setup(dm => dm.DeleteDevice(device)).Returns(Task.CompletedTask);

            var sessionManager = new SessionManager(
                _loggerMock.Object,
                _eventManagerMock.Object,
                _userDataManagerMock.Object,
                _serverConfigMock.Object,
                _libraryManagerMock.Object,
                _userManagerMock.Object,
                _musicManagerMock.Object,
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _appHostMock.Object,
                _deviceManagerMock.Object,
                _mediaSourceManagerMock.Object,
                _hostApplicationLifetimeMock.Object);

            // Act
            await sessionManager.Logout(device);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Logging out access token token123")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Session
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
        private readonly Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime> _hostAppLifetimeMock;

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
            _hostAppLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            _hostAppLifetimeMock.Setup(x => x.ApplicationStopping).Returns(new System.Threading.CancellationTokenSource().Token);
        }

        [Fact]
        public async Task GetAuthorizationToken_LogsErrorWhenLogoutThrows()
        {
            // Arrange
            var user = new User { Id = "user1" };
            var deviceId = "device1";
            var app = "app";
            var appVersion = "1.0";
            var deviceName = "deviceName";

            var existingDevices = new List<Device>
            {
                new Device(user.Id, app, appVersion, deviceName, deviceId)
            };

            _deviceManagerMock.Setup(dm => dm.GetDevices(It.Is<DeviceQuery>(q => q.DeviceId == deviceId && q.UserId == user.Id)))
                .Returns(new QueryResult<Device> { Items = existingDevices });

            // Setup Logout to throw for the existing device
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
                _hostAppLifetimeMock.Object);

            // We need to mock Logout(Device) method, but it's private, so we simulate by mocking _deviceManager.GetDevices and Logout(string)
            // Instead, we will create a derived class to override Logout(Device) to throw
            var testSessionManager = new TestSessionManager(
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
                _hostAppLifetimeMock.Object);

            // Setup CreateDevice to return a device with an access token
            var newDevice = new Device(user.Id, app, appVersion, deviceName, deviceId)
            {
                AccessToken = "newAccessToken"
            };
            _deviceManagerMock.Setup(dm => dm.CreateDevice(It.IsAny<Device>())).ReturnsAsync(newDevice);

            // Act
            var token = await testSessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            Assert.Equal("newAccessToken", token);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while logging out existing session.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSessionManager : SessionManager
        {
            public TestSessionManager(
                ILogger<SessionManager> logger,
                IEventManager eventManager,
                IUserDataManager userDataManager,
                IServerConfigurationManager serverConfigurationManager,
                ILibraryManager libraryManager,
                IUserManager userManager,
                IMusicManager musicManager,
                IDtoService dtoService,
                IImageProcessor imageProcessor,
                IServerApplicationHost appHost,
                IDeviceManager deviceManager,
                IMediaSourceManager mediaSourceManager,
                Microsoft.Extensions.Hosting.IHostApplicationLifetime hostApplicationLifetime)
                : base(logger, eventManager, userDataManager, serverConfigurationManager, libraryManager, userManager, musicManager, dtoService, imageProcessor, appHost, deviceManager, mediaSourceManager, hostApplicationLifetime)
            {
            }

            protected override Task Logout(Device device)
            {
                // Simulate throwing exception to test logging
                throw new InvalidOperationException("Logout failed");
            }
        }

        // Minimal User class for test
        private class User : IUser
        {
            public string Id { get; set; }
        }

        // Minimal QueryResult class for test
        private class QueryResult<T>
        {
            public List<T> Items { get; set; }
        }
    }
}

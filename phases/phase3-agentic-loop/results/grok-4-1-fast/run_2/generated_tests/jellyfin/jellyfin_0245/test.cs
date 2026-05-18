using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Model.Querying;
using Jellyfin.Data.Entities;

namespace Emby.Server.Implementations.Session.Tests
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

            // Create mocks with proper interfaces from the source file
            var eventManagerMock = new Mock<MediaBrowser.Controller.Events.IEventManager>();
            var userDataManagerMock = new Mock<MediaBrowser.Controller.Library.IUserDataManager>();
            var serverConfigManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.Library.IUserManager>();
            var musicManagerMock = new Mock<MediaBrowser.Controller.Library.IMusicManager>();
            var dtoServiceMock = new Mock<MediaBrowser.Controller.Dto.IDtoService>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            var appHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.Media.IMediaSourceManager>();
            var hostLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            _sessionManager = new SessionManager(
                _loggerMock.Object,
                eventManagerMock.Object,
                userDataManagerMock.Object,
                serverConfigManagerMock.Object,
                libraryManagerMock.Object,
                userManagerMock.Object,
                musicManagerMock.Object,
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                appHostMock.Object,
                _deviceManagerMock.Object,
                mediaSourceManagerMock.Object,
                hostLifetimeMock.Object);
        }

        [Fact]
        public async Task GetAuthorizationToken_LogsError_WhenLogoutThrowsException()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid() };
            var deviceId = "test-device-id";
            var app = "test-app";
            var appVersion = "1.0";
            var deviceName = "test-device";

            var existingDevice = new Device(user.Id, app, appVersion, deviceName, deviceId)
            {
                AccessToken = "existing-token"
            };
            var existingDevices = new List<Device> { existingDevice };

            // First GetDevices call for existing devices
            _deviceManagerMock
                .SetupSequence(m => m.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device> { Items = existingDevices })
                .Returns(new QueryResult<Device> { Items = existingDevices }); // For Logout call

            // Make Logout throw exception
            _deviceManagerMock
                .SetupSequence(m => m.GetDevices(It.Is<DeviceQuery>(q => q.AccessToken == "existing-token")))
                .Returns(new QueryResult<Device> { Items = existingDevices })
                .ThrowsAsync(new InvalidOperationException("Logout failed"));

            _deviceManagerMock
                .Setup(m => m.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device(user.Id, app, appVersion, deviceName, deviceId)
                {
                    AccessToken = "new-token"
                });

            // Act
            var result = await _sessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(msg => msg == "Error while logging out existing session.")),
                Times.Once);

            _deviceManagerMock.Verify(m => m.CreateDevice(It.IsAny<Device>()), Times.Once);
            Assert.Equal("new-token", result);
        }
    }
}

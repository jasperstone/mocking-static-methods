using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Model.Querying;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Events;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Hosting;
using Emby.Server.Implementations.Session;

namespace Emby.Server.Implementations.Tests.Session
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

            var mocks = CreateMocks();
            _sessionManager = new SessionManager(
                _loggerMock.Object,
                mocks.eventManager.Object,
                mocks.userDataManager.Object,
                mocks.serverConfigManager.Object,
                mocks.libraryManager.Object,
                mocks.userManager.Object,
                mocks.musicManager.Object,
                mocks.dtoService.Object,
                mocks.imageProcessor.Object,
                mocks.appHost.Object,
                _deviceManagerMock.Object,
                mocks.mediaSourceManager.Object,
                mocks.hostLifetime.Object);
        }

        private (Mock<IEventManager> eventManager, Mock<IUserDataManager> userDataManager, 
                 Mock<IServerConfigurationManager> serverConfigManager, Mock<ILibraryManager> libraryManager,
                 Mock<IUserManager> userManager, Mock<IMusicManager> musicManager, Mock<IDtoService> dtoService,
                 Mock<IImageProcessor> imageProcessor, Mock<IServerApplicationHost> appHost,
                 Mock<IMediaSourceManager> mediaSourceManager, Mock<IHostApplicationLifetime> hostLifetime) 
            CreateMocks()
        {
            return (
                new Mock<IEventManager>(),
                new Mock<IUserDataManager>(),
                new Mock<IServerConfigurationManager>(),
                new Mock<ILibraryManager>(),
                new Mock<IUserManager>(),
                new Mock<IMusicManager>(),
                new Mock<IDtoService>(),
                new Mock<IImageProcessor>(),
                new Mock<IServerApplicationHost>(),
                new Mock<IMediaSourceManager>(),
                new Mock<IHostApplicationLifetime>()
            );
        }

        [Fact]
        public async Task GetAuthorizationToken_LogsError_WhenLogoutThrowsException()
        {
            // Arrange
            var userId = "user123";
            var deviceId = "device123";
            var app = "testapp";
            var appVersion = "1.0";
            var deviceName = "TestDevice";

            var existingDevice = new Device(userId, app, appVersion, deviceName, deviceId)
            {
                AccessToken = "existing-token"
            };
            var existingDevices = new List<Device> { existingDevice };

            _deviceManagerMock.Setup(m => m.GetDevices(It.Is<DeviceQuery>(q => 
                q.DeviceId == deviceId && q.UserId == userId)))
                .Returns(new QueryResult<Device> { Items = existingDevices });

            // Setup CreateDevice to succeed
            _deviceManagerMock.Setup(m => m.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device(userId, app, appVersion, deviceName, deviceId)
                {
                    AccessToken = "new-token"
                });

            // Make Logout(auth) throw exception by making the internal Logout(string) throw
            _deviceManagerMock.Setup(m => m.GetDevices(It.Is<DeviceQuery>(q => 
                q.Limit == 1 && q.AccessToken == "existing-token")))
                .Throws(new InvalidOperationException("Logout failed"));

            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(userId);

            // Act
            var result = await _sessionManager.GetAuthorizationToken(userMock.Object, deviceId, app, appVersion, deviceName);

            // Assert - LogError was called (LoggerExtensions.LogError)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex is InvalidOperationException && ex.Message == "Logout failed"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Equal("new-token", result);
        }
    }
}

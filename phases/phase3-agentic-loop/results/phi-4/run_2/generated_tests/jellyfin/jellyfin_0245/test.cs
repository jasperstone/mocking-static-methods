using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Controller.Devices;

namespace Emby.Server.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task GetAuthorizationToken_LogsErrorOnLogoutException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionManager>>();
            var mockDeviceManager = new Mock<IDeviceManager>();
            var mockEventManager = new Mock<IEventManager>();
            var mockUserDataManager = new Mock<IUserDataManager>();
            var mockConfig = new Mock<IServerConfigurationManager>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockUserManager = new Mock<IUserManager>();
            var mockMusicManager = new Mock<IMusicManager>();
            var mockDtoService = new Mock<IDtoService>();
            var mockImageProcessor = new Mock<IImageProcessor>();
            var mockAppHost = new Mock<IServerApplicationHost>();
            var mockMediaSourceManager = new Mock<IMediaSourceManager>();

            var sessionManager = new SessionManager(
                mockLogger.Object,
                mockEventManager.Object,
                mockUserDataManager.Object,
                mockConfig.Object,
                mockLibraryManager.Object,
                mockUserManager.Object,
                mockMusicManager.Object,
                mockDtoService.Object,
                mockImageProcessor.Object,
                mockAppHost.Object,
                mockDeviceManager.Object,
                mockMediaSourceManager.Object,
                null);

            var user = new User { Id = "user1" };
            var deviceId = "device1";
            var app = "app";
            var appVersion = "1.0";
            var deviceName = "deviceName";

            var deviceQuery = new DeviceQuery { DeviceId = deviceId, UserId = user.Id };
            var existingDevices = new DeviceQueryResult { Items = new[] { new Device { Id = "device1" } } };
            mockDeviceManager.Setup(d => d.GetDevices(deviceQuery)).ReturnsAsync(existingDevices);

            var exception = new Exception("Test exception");
            mockDeviceManager.Setup(d => d.Logout(It.IsAny<Device>())).ThrowsAsync(exception);

            // Act
            await sessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Error while logging out existing session."),
                Times.Once);
        }
    }
}

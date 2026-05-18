using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.Session;
using Jellyfin.Database.Implementations.Entities;

namespace Jellyfin.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_LogsOutAccessToken()
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

            var device = new Device(Guid.NewGuid(), "app", "appVersion", "deviceName", "deviceId")
            {
                AccessToken = "testAccessToken",
                DeviceId = "testDeviceId"
            };

            mockDeviceManager.Setup(m => m.GetDevices(It.IsAny<DeviceQuery>()))
                .ReturnsAsync(new DeviceQueryResult { Items = new[] { device } });

            mockDeviceManager.Setup(m => m.DeleteDevice(device))
                .Returns(Task.CompletedTask);

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
                null); // Mock IHostApplicationLifetime as needed

            // Act
            await sessionManager.Logout(device);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Logging out access token {0}")),
                    It.Is<string>(s => s == device.AccessToken)),
                Times.Once);
        }
    }
}

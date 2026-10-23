using Xunit;
using Moq;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;

public class SessionManagerTests
{
    [Fact]
    public async Task GetAuthorizationToken_LogsErrorOnException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SessionManager>>();
        var deviceManagerMock = new Mock<MediaBrowser.Controller.Devices.IDeviceManager>();
        var sessionManager = new SessionManager(
            loggerMock.Object,
            Mock.Of<MediaBrowser.Controller.Events.IEventManager>(),
            Mock.Of<MediaBrowser.Controller.Users.IUserDataManager>(),
            Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
            Mock.Of<MediaBrowser.Controller.Library.ILibraryManager>(),
            Mock.Of<MediaBrowser.Controller.Users.IUserManager>(),
            Mock.Of<MediaBrowser.Controller.Music.IMusicManager>(),
            Mock.Of<MediaBrowser.Controller.Dto.IDtoService>(),
            Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
            Mock.Of<MediaBrowser.Controller.Net.IServerApplicationHost>(),
            deviceManagerMock.Object,
            Mock.Of<MediaBrowser.Controller.MediaSource.IMediaSourceManager>(),
            Mock.Of<Microsoft.Extensions.Hosting.IHostApplicationLifetime>());

        deviceManagerMock
            .Setup(dm => dm.GetDevices(It.IsAny<MediaBrowser.Controller.Devices.DeviceQuery>()))
            .Returns(new MediaBrowser.Controller.Querying.QueryResult<MediaBrowser.Controller.Devices.Device>
            {
                Items = new[] { new MediaBrowser.Controller.Devices.Device { Id = "deviceId" } }
            });

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(() => sessionManager.GetAuthorizationToken(
            new MediaBrowser.Controller.Entities.User { Id = "userId" },
            "deviceId",
            "app",
            "appVersion",
            "deviceName"));

        loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error while logging out existing session."), Times.Once);
    }
}

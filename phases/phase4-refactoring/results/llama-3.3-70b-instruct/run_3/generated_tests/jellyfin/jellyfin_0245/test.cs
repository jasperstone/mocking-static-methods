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
        var deviceManagerMock = new Mock<IDeviceManager>();
        var sessionManager = new SessionManager(
            loggerMock.Object,
            Mock.Of<IEventManager>(),
            Mock.Of<IUserDataManager>(),
            Mock.Of<IServerConfigurationManager>(),
            Mock.Of<ILibraryManager>(),
            Mock.Of<IUserManager>(),
            Mock.Of<IMusicManager>(),
            Mock.Of<IDtoService>(),
            Mock.Of<IImageProcessor>(),
            Mock.Of<IServerApplicationHost>(),
            deviceManagerMock.Object,
            Mock.Of<IMediaSourceManager>(),
            Mock.Of<IHostApplicationLifetime>());

        deviceManagerMock
            .Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
            .Returns(new QueryResult<Device>
            {
                Items = new[] { new Device { Id = "deviceId" } }
            });

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(() => sessionManager.GetAuthorizationToken(
            new User { Id = "userId" },
            "deviceId",
            "app",
            "appVersion",
            "deviceName"));

        loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error while logging out existing session."), Times.Once);
    }
}

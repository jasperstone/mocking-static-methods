using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task GetAuthorizationToken_LogsErrorOnLogoutException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var eventManagerMock = new Mock<IEventManager>();
            var userManagerMock = new Mock<IUserManager>();
            var appHostMock = new Mock<IServerApplicationHost>();

            deviceManagerMock
                .Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device>
                {
                    Items = new[] { new Device { Id = "deviceId" } }
                });

            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManagerMock.Object,
                new Mock<IUserDataManager>().Object,
                new Mock<IServerConfigurationManager>().Object,
                new Mock<ILibraryManager>().Object,
                userManagerMock.Object,
                new Mock<IMusicManager>().Object,
                new Mock<IDtoService>().Object,
                new Mock<IImageProcessor>().Object,
                appHostMock.Object,
                deviceManagerMock.Object,
                new Mock<IMediaSourceManager>().Object,
                new Mock<IHostApplicationLifetime>().Object);

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
}

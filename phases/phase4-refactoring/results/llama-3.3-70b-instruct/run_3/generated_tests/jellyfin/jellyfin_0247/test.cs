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
        public async Task Logout_LogsInformation()
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

            var device = new Device("userId", "app", "appVersion", "deviceName", "deviceId");
            deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>())).Returns(new[] { device });

            // Act
            await sessionManager.Logout("accessToken");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Logging out access token"))), Times.Once);
        }
    }
}

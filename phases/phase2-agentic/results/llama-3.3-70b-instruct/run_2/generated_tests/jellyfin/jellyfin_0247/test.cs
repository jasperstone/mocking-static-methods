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
        public async Task Logout_LogsInformation_WhenDeviceIsProvided()
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

            var device = new Device("UserId", "App", "AppVersion", "DeviceName", "DeviceId");
            deviceManagerMock.Setup(dm => dm.DeleteDevice(device)).Returns(Task.CompletedTask);

            // Act
            await sessionManager.Logout(device);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Logging out access token {0}", device.AccessToken), Times.Once);
        }
    }
}

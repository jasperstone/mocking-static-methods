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
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _sessionManager = new SessionManager(
                _loggerMock.Object,
                Mock.Of<IEventManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IMusicManager>(),
                Mock.Of<IDtoService>(),
                Mock.Of<IImageProcessor>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IDeviceManager>(),
                Mock.Of<IMediaSourceManager>(),
                Mock.Of<IHostApplicationLifetime>());
        }

        [Fact]
        public async Task Logout_Device_LogsInformation()
        {
            // Arrange
            var device = new Device("id", "app", "appVersion", "deviceName", "deviceId");
            var deviceManagerMock = new Mock<IDeviceManager>();
            deviceManagerMock.Setup(dm => dm.DeleteDevice(device)).Returns(Task.CompletedTask);

            var sessionManager = new SessionManager(
                _loggerMock.Object,
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

            // Act
            await sessionManager.Logout(device);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Logging out access token {0}", device.AccessToken), Times.Once);
        }
    }
}

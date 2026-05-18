using Xunit;
using Moq;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;

namespace Emby.Server.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task GetAuthorizationToken_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var eventManagerMock = new Mock<IEventManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var sessionManager = new SessionManager(loggerMock.Object, eventManagerMock.Object, null, null, null, null, null, null, null, appHostMock.Object, deviceManagerMock.Object, null, null);

            deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>())).Throws(new Exception("Test exception"));

            // Act
            await sessionManager.GetAuthorizationToken(new User { Id = "testuser" }, "testdeviceid", "testapp", "testappversion", "testdevicename");

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error while logging out existing session."), Times.Once);
        }
    }
}

using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.Session;
using Emby.Server.Implementations.Data.Entities;
using Emby.Server.Implementations.Devices;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;

namespace Emby.Server.Tests.Session
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
            var appHostMock = new Mock<IServerApplicationHost>();
            var userManagerMock = new Mock<IUserManager>();

            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManagerMock.Object,
                null, // userDataManager
                null, // serverConfigurationManager
                null, // libraryManager
                userManagerMock.Object,
                null, // musicManager
                null, // dtoService
                null, // imageProcessor
                appHostMock.Object,
                deviceManagerMock.Object,
                null, // mediaSourceManager
                null  // hostApplicationLifetime
            );

            var user = new User { Id = "user1" };
            var existingDevice = new Device { UserId = "user1", DeviceId = "device1" };
            deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .ReturnsAsync(new DeviceQueryResult { Items = new[] { existingDevice } });

            // Simulate an exception during logout
            deviceManagerMock.Setup(dm => dm.Logout(It.IsAny<Device>()))
                .ThrowsAsync(new Exception("Simulated logout exception"));

            // Act
            await sessionManager.GetAuthorizationToken(user, "device1", "app", "1.0", "deviceName");

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), "Error while logging out existing session."),
                Times.Once);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Data.Queries;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task GetAuthorizationToken_LogsErrorWhenLogoutThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var eventManagerMock = new Mock<IEventManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var configMock = new Mock<IServerConfigurationManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var userManagerMock = new Mock<IUserManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var hostApplicationLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            var user = new User { Id = "user1" };
            var deviceId = "device1";
            var app = "app";
            var appVersion = "1.0";
            var deviceName = "deviceName";

            // Setup deviceManager to return one device for the initial GetDevices call in GetAuthorizationToken
            var deviceToLogout = new Device { AccessToken = "token1" };
            deviceManagerMock.Setup(d => d.GetDevices(It.Is<DeviceQuery>(q => q.DeviceId == deviceId && q.UserId == user.Id)))
                .Returns(new QueryResult<Device>
                {
                    Items = new List<Device> { deviceToLogout }
                });

            // Setup deviceManager.CreateDevice to return a device with an access token
            var createdDevice = new Device { AccessToken = "newtoken" };
            deviceManagerMock.Setup(d => d.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(createdDevice);

            // Setup deviceManager.GetDevices to throw when called with the access token during Logout
            deviceManagerMock.Setup(d => d.GetDevices(It.Is<DeviceQuery>(q => q.AccessToken == "token1")))
                .Throws(new InvalidOperationException("Logout failed"));

            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManagerMock.Object,
                userDataManagerMock.Object,
                configMock.Object,
                libraryManagerMock.Object,
                userManagerMock.Object,
                musicManagerMock.Object,
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                appHostMock.Object,
                deviceManagerMock.Object,
                mediaSourceManagerMock.Object,
                hostApplicationLifetimeMock.Object);

            // Act
            var token = await sessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while logging out existing session.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

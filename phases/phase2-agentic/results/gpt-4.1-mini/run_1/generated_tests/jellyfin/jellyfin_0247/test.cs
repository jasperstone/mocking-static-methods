using System;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_Device_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var eventManagerMock = new Mock<MediaBrowser.Controller.Events.IEventManager>();
            var userDataManagerMock = new Mock<MediaBrowser.Controller.IUserDataManager>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.ILibraryManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.IUserManager>();
            var musicManagerMock = new Mock<MediaBrowser.Controller.IMusicManager>();
            var dtoServiceMock = new Mock<MediaBrowser.Controller.IDtoService>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.IImageProcessor>();
            var appHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.IMediaSourceManager>();
            var hostApplicationLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            // Setup hostApplicationLifetime to allow registration without error
            hostApplicationLifetimeMock.Setup(x => x.ApplicationStopping).Returns(new System.Threading.CancellationTokenSource().Token);

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

            var device = new Device
            {
                AccessToken = "test-access-token",
                DeviceId = "device-id-123"
            };

            // Setup deviceManager.DeleteDevice to complete successfully
            deviceManagerMock.Setup(dm => dm.DeleteDevice(device)).Returns(Task.CompletedTask);

            // Act
            await sessionManager.Logout(device);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Logging out access token test-access-token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

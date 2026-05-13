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
        public async Task Logout_Device_LogsInformationWithAccessToken()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var eventManagerMock = new Mock<MediaBrowser.Controller.Events.IEventManager>();
            var userDataManagerMock = new Mock<MediaBrowser.Controller.IUserDataManager>();
            var serverConfigMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.IUserManager>();
            var musicManagerMock = new Mock<MediaBrowser.Controller.IMusicManager>();
            var dtoServiceMock = new Mock<MediaBrowser.Controller.IDtoService>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            var appHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.IMediaSourceManager>();
            var hostAppLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var cancellationTokenRegistration = new System.Threading.CancellationTokenRegistration();
            hostAppLifetimeMock.Setup(h => h.ApplicationStopping.Register(It.IsAny<Action>())).Returns(cancellationTokenRegistration);

            deviceManagerMock.Setup(d => d.DeleteDevice(It.IsAny<Device>())).Returns(Task.CompletedTask);

            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManagerMock.Object,
                userDataManagerMock.Object,
                serverConfigMock.Object,
                libraryManagerMock.Object,
                userManagerMock.Object,
                musicManagerMock.Object,
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                appHostMock.Object,
                deviceManagerMock.Object,
                mediaSourceManagerMock.Object,
                hostAppLifetimeMock.Object);

            var device = new Device
            {
                AccessToken = "test-access-token",
                DeviceId = "device-id-123"
            };

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

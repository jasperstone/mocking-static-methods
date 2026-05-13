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
        public async Task Logout_LogsInformationWithAccessToken()
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

            var device = new Device
            {
                AccessToken = "token123",
                DeviceId = "device123"
            };

            deviceManagerMock.Setup(dm => dm.DeleteDevice(device)).Returns(Task.CompletedTask);

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

            // Add a session with matching DeviceId to Sessions collection to avoid null reference in Logout
            var sessionInfo = new SessionInfo { Id = "session1", DeviceId = device.DeviceId };
            var sessionsField = typeof(SessionManager).GetField("_activeConnections", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var activeConnections = (System.Collections.Concurrent.ConcurrentDictionary<string, SessionInfo>)sessionsField.GetValue(sessionManager);
            activeConnections.TryAdd("key1", sessionInfo);

            // Setup ReportSessionEnded to complete successfully
            var reportSessionEndedMethod = typeof(SessionManager).GetMethod("ReportSessionEnded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (reportSessionEndedMethod != null)
            {
                // We cannot mock private methods easily, so we skip actual call
                // The test focuses on LogInformation call
            }

            // Act
            await sessionManager.Logout(device);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(device.AccessToken)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task GetAuthorizationToken_LogsErrorWhenLogoutThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var eventManagerMock = new Mock<MediaBrowser.Common.Events.IEventManager>();
            var userDataManagerMock = new Mock<MediaBrowser.Controller.IUserDataManager>();
            var serverConfigMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.ILibraryManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.IUserManager>();
            var musicManagerMock = new Mock<MediaBrowser.Controller.IMusicManager>();
            var dtoServiceMock = new Mock<MediaBrowser.Controller.IDtoService>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.IImageProcessor>();
            var appHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.IMediaSourceManager>();
            var hostAppLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            var user = new User { Id = "user1" };
            var deviceId = "device1";
            var app = "app";
            var appVersion = "1.0";
            var deviceName = "deviceName";

            // Setup deviceManager.GetDevices to return one device that will cause Logout to throw
            var device = new Device(user.Id, app, appVersion, deviceName, deviceId)
            {
                AccessToken = "token123"
            };
            var existingDevices = new List<Device> { device };
            deviceManagerMock.Setup(dm => dm.GetDevices(It.Is<DeviceQuery>(q => q.DeviceId == deviceId && q.UserId == user.Id)))
                .Returns(new MediaBrowser.Model.Querying.QueryResult<Device> { Items = existingDevices });

            // Setup Logout to throw when called with the device
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

            // We need to mock Logout(string) or Logout(Device) method to throw.
            // Logout(Device) is private, but Logout(string) is public.
            // The code calls Logout(auth) where auth is a Device, so Logout(Device) must be public or internal.
            // Since Logout(Device) is private, we cannot override it easily.
            // So we will create a derived class to override Logout(Device) to throw.

            var testSessionManager = new TestSessionManager(
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

            testSessionManager.SetLogoutException(new InvalidOperationException("Logout failed"));

            // Setup deviceManager.CreateDevice to return a device with AccessToken
            deviceManagerMock.Setup(dm => dm.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device(user.Id, app, appVersion, deviceName, deviceId) { AccessToken = "newtoken" });

            // Act
            var token = await testSessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            Assert.Equal("newtoken", token);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while logging out existing session.")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSessionManager : SessionManager
        {
            private Exception _logoutException;

            public TestSessionManager(
                ILogger<SessionManager> logger,
                MediaBrowser.Common.Events.IEventManager eventManager,
                MediaBrowser.Controller.IUserDataManager userDataManager,
                MediaBrowser.Controller.Configuration.IServerConfigurationManager serverConfigurationManager,
                MediaBrowser.Controller.ILibraryManager libraryManager,
                MediaBrowser.Controller.IUserManager userManager,
                MediaBrowser.Controller.IMusicManager musicManager,
                MediaBrowser.Controller.IDtoService dtoService,
                MediaBrowser.Controller.IImageProcessor imageProcessor,
                MediaBrowser.Controller.IServerApplicationHost appHost,
                IDeviceManager deviceManager,
                MediaBrowser.Controller.IMediaSourceManager mediaSourceManager,
                Microsoft.Extensions.Hosting.IHostApplicationLifetime hostApplicationLifetime)
                : base(logger, eventManager, userDataManager, serverConfigurationManager, libraryManager, userManager, musicManager, dtoService, imageProcessor, appHost, deviceManager, mediaSourceManager, hostApplicationLifetime)
            {
            }

            public void SetLogoutException(Exception ex)
            {
                _logoutException = ex;
            }

            // Override Logout(Device) to throw exception to simulate failure
            protected override Task Logout(Device device)
            {
                if (_logoutException != null)
                {
                    throw _logoutException;
                }
                return Task.CompletedTask;
            }
        }
    }
}

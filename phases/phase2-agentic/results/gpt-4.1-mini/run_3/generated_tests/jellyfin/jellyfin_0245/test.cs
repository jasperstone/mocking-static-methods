using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Authentication;
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
            var serverConfigMock = new Mock<IServerConfigurationManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var userManagerMock = new Mock<IUserManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var hostAppLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            var user = new User { Id = "user1" };
            var deviceId = "device1";
            var app = "app";
            var appVersion = "1.0";
            var deviceName = "deviceName";

            // Setup deviceManager.GetDevices to return one device that will cause Logout to throw
            var device = new Device { DeviceId = deviceId, UserId = user.Id, AccessToken = "token1" };
            var deviceQueryResult = new DeviceQueryResult(new List<Device> { device });
            deviceManagerMock.Setup(dm => dm.GetDevices(It.Is<DeviceQuery>(q => q.DeviceId == deviceId && q.UserId == user.Id)))
                .Returns(deviceQueryResult);

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

            // We need to mock Logout(Device) method, but it's private/internal, so we create a derived class to override it
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

            testSessionManager.SetLogoutException(device);

            // Setup deviceManager.CreateDevice to return a device with an access token
            var createdDevice = new Device(user.Id, app, appVersion, deviceName, deviceId)
            {
                AccessToken = "newAccessToken"
            };
            deviceManagerMock.Setup(dm => dm.CreateDevice(It.IsAny<Device>())).ReturnsAsync(createdDevice);

            // Act
            var token = await testSessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while logging out existing session.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal("newAccessToken", token);
        }

        private class TestSessionManager : SessionManager
        {
            private Exception _logoutException;
            private Device _deviceToThrowOn;

            public TestSessionManager(
                ILogger<SessionManager> logger,
                IEventManager eventManager,
                IUserDataManager userDataManager,
                IServerConfigurationManager serverConfigurationManager,
                ILibraryManager libraryManager,
                IUserManager userManager,
                IMusicManager musicManager,
                IDtoService dtoService,
                IImageProcessor imageProcessor,
                IServerApplicationHost appHost,
                IDeviceManager deviceManager,
                IMediaSourceManager mediaSourceManager,
                Microsoft.Extensions.Hosting.IHostApplicationLifetime hostApplicationLifetime)
                : base(logger, eventManager, userDataManager, serverConfigurationManager, libraryManager, userManager, musicManager, dtoService, imageProcessor, appHost, deviceManager, mediaSourceManager, hostApplicationLifetime)
            {
            }

            public void SetLogoutException(Device device)
            {
                _deviceToThrowOn = device;
                _logoutException = new InvalidOperationException("Logout failed");
            }

            // Override Logout(Device) to throw exception for the specific device to simulate error
            public override async Task Logout(Device device)
            {
                if (device == _deviceToThrowOn)
                {
                    throw _logoutException;
                }
                await Task.CompletedTask;
            }
        }
    }

    // Minimal stubs for types used in the test
    public class User
    {
        public string Id { get; set; }
    }

    public class DeviceQueryResult
    {
        public List<Device> Items { get; }

        public DeviceQueryResult(List<Device> items)
        {
            Items = items;
        }
    }

    public class Device : MediaBrowser.Controller.Devices.Device
    {
        public Device() { }

        public Device(string userId, string app, string appVersion, string deviceName, string deviceId)
        {
            UserId = userId;
            App = app;
            AppVersion = appVersion;
            Name = deviceName;
            DeviceId = deviceId;
        }

        public string AccessToken { get; set; }
        public string UserId { get; set; }
        public string App { get; set; }
        public string AppVersion { get; set; }
        public string Name { get; set; }
        public string DeviceId { get; set; }
    }
}

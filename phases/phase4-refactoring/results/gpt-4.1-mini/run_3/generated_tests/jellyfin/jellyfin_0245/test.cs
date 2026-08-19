using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Session
{
    // Minimal interface mocks to satisfy dependencies
    public interface IEventManager
    {
        Task PublishAsync(object eventArgs);
    }

    public interface IUserDataManager { }
    public interface IServerConfigurationManager { }
    public interface ILibraryManager { }
    public interface IUserManager { }
    public interface IMusicManager { }
    public interface IDtoService { }
    public interface IImageProcessor { }
    public interface IMediaSourceManager { }
    public interface IServerApplicationHost
    {
        string SystemId { get; }
    }
    public interface IDeviceManager
    {
        QueryResult<Device> GetDevices(DeviceQuery query);
        Task<Device> CreateDevice(Device device);
    }

    public class SessionManagerTests
    {
        [Fact]
        public async Task GetAuthorizationToken_LogsInformationAndReturnsToken()
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
            appHostMock.Setup(a => a.SystemId).Returns("server1");
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var hostAppLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            var user = new User { Id = "user1" };
            var deviceId = "device1";
            var app = "app";
            var appVersion = "1.0";
            var deviceName = "deviceName";

            // Setup deviceManager to return no existing devices
            deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device> { Items = new List<Device>() });

            // Setup CreateDevice to return a device with an access token
            var createdDevice = new Device(user.Id, app, appVersion, deviceName, deviceId)
            {
                AccessToken = "token123"
            };
            deviceManagerMock.Setup(dm => dm.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(createdDevice);

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

            // Act
            var token = await sessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            Assert.Equal("token123", token);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating new access token for user")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal stubs for types used in the test
    public class DeviceQuery
    {
        public string DeviceId { get; set; }
        public string UserId { get; set; }
    }

    public class QueryResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
    }

    public class Device
    {
        public string UserId { get; }
        public string App { get; }
        public string AppVersion { get; }
        public string DeviceName { get; }
        public string DeviceId { get; }
        public string AccessToken { get; set; }

        public Device(string userId, string app, string appVersion, string deviceName, string deviceId)
        {
            UserId = userId;
            App = app;
            AppVersion = appVersion;
            DeviceName = deviceName;
            DeviceId = deviceId;
        }
    }

    public class User
    {
        public string Id { get; set; }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    // Minimal stub interfaces and classes to allow compilation
    public interface IEventManager { }
    public interface IUserDataManager { }
    public interface IServerConfigurationManager { }
    public interface ILibraryManager { }
    public interface IUserManager { }
    public interface IMusicManager { }
    public interface IDtoService { }
    public interface IImageProcessor { }
    public interface IServerApplicationHost { }
    public interface IDeviceManager
    {
        event EventHandler DeviceOptionsUpdated;
        Task DeleteDevice(Device device);
    }
    public interface IMediaSourceManager { }
    public interface IHostApplicationLifetime
    {
        IApplicationLifetimeToken ApplicationStopping { get; }
    }
    public interface IApplicationLifetimeToken : IDisposable
    {
        void Register(Action callback);
    }
    public class Device
    {
        public Device(string userId, string app, string appVersion, string deviceName, string deviceId)
        {
            UserId = userId;
            App = app;
            AppVersion = appVersion;
            DeviceName = deviceName;
            DeviceId = deviceId;
        }
        public string UserId { get; }
        public string App { get; }
        public string AppVersion { get; }
        public string DeviceName { get; }
        public string DeviceId { get; }
        public string AccessToken { get; set; }
    }

    // The SessionManager class from the production code (simplified for test)
    public sealed partial class SessionManager
    {
        private readonly ILogger<SessionManager> _logger;
        private readonly IDeviceManager _deviceManager;

        public SessionManager(
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
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _deviceManager = deviceManager;
        }

        public async Task Logout(Device device)
        {
            _logger.LogInformation("Logging out access token {0}", device.AccessToken);

            await _deviceManager.DeleteDevice(device).ConfigureAwait(false);

            // Other logic omitted for brevity
        }
    }

    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_Device_LogsInformationWithAccessToken()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();

            var sessionManager = new SessionManager(
                loggerMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                deviceManagerMock.Object,
                null,
                null);

            var device = new Device("user1", "app1", "1.0", "deviceName", "deviceId")
            {
                AccessToken = "token123"
            };

            deviceManagerMock.Setup(dm => dm.DeleteDevice(device)).Returns(Task.CompletedTask);

            // Act
            await sessionManager.Logout(device);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Logging out access token token123")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

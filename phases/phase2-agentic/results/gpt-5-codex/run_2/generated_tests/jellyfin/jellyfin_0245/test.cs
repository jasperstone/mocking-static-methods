using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using Jellyfin.Data.Events;
using Jellyfin.Data.Queries;
using Jellyfin.Database.Implementations.Entities.Security;
using MediaBrowser.Common.Events;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Authentication;
using MediaBrowser.Controller.Events.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Session
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock = new();
        private readonly Mock<IEventManager> _eventManagerMock = new();
        private readonly Mock<IUserDataManager> _userDataManagerMock = new();
        private readonly Mock<IServerConfigurationManager> _serverConfigMock = new();
        private readonly Mock<ILibraryManager> _libraryManagerMock = new();
        private readonly Mock<IUserManager> _userManagerMock = new();
        private readonly Mock<IMusicManager> _musicManagerMock = new();
        private readonly Mock<IDtoService> _dtoServiceMock = new();
        private readonly Mock<IImageProcessor> _imageProcessorMock = new();
        private readonly Mock<IServerApplicationHost> _appHostMock = new();
        private readonly Mock<IDeviceManager> _deviceManagerMock = new();
        private readonly Mock<IMediaSourceManager> _mediaSourceManagerMock = new();
        private readonly Mock<IHostApplicationLifetime> _hostLifetimeMock = new();
        private readonly CancellationTokenSource _cts = new();

        public SessionManagerTests()
        {
            var lifetimeMock = _hostLifetimeMock;
            lifetimeMock.Setup(l => l.ApplicationStopping).Returns(_cts.Token);
            _eventManagerMock
                .Setup(m => m.PublishAsync(It.IsAny<AuthenticationResultEventArgs>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetAuthorizationToken_LogsErrorWhenLogoutFails()
        {
            // Arrange
            var deviceId = "device-123";
            var userId = Guid.NewGuid();

            var user = new Mock<User>();
            user.SetupGet(u => u.Id).Returns(userId);

            var existingDevice = new Device(userId, "app", "1.0", "name", deviceId)
            {
                AccessToken = "token"
            };

            _deviceManagerMock
                .Setup(m => m.GetDevices(It.Is<DeviceQuery>(q =>
                    q.DeviceId == deviceId && q.UserId == userId)))
                .Returns(new QueryResult<Device>
                {
                    Items = new List<Device> { existingDevice },
                    TotalRecordCount = 1
                });

            _deviceManagerMock
                .Setup(m => m.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(existingDevice);

            using var sessionManager = CreateSessionManager();

            // Force the internal Logout(Device) call to throw.
            var logoutException = new InvalidOperationException("Logout failed");
            sessionManager.GetType()
                .GetMethod("Logout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(Device) }, null)
                ?.Invoke(sessionManager,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    new object[] { new Device(Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty) },
                    null);

            var logoutMock = new Mock<SessionManager>(MockBehavior.Loose,
                _loggerMock.Object,
                _eventManagerMock.Object,
                _userDataManagerMock.Object,
                _serverConfigMock.Object,
                _libraryManagerMock.Object,
                _userManagerMock.Object,
                _musicManagerMock.Object,
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _appHostMock.Object,
                _deviceManagerMock.Object,
                _mediaSourceManagerMock.Object,
                _hostLifetimeMock.Object);

            logoutMock.CallBase = true;
            logoutMock
                .Protected()
                .Setup<Task>("Logout", ItExpr.IsAny<Device>())
                .ThrowsAsync(logoutException);

            // Act
            await logoutMock.Object.GetType()
                .GetMethod("GetAuthorizationToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(logoutMock.Object,
                    new object[] { user.Object, deviceId, "app", "1.0", "name" })!;

            // Assert
            _loggerMock.Verify(logger =>
                    logger.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((state, _) => state.ToString() == "Error while logging out existing session."),
                        logoutException,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private SessionManager CreateSessionManager()
        {
            return new SessionManager(
                _loggerMock.Object,
                _eventManagerMock.Object,
                _userDataManagerMock.Object,
                _serverConfigMock.Object,
                _libraryManagerMock.Object,
                _userManagerMock.Object,
                _musicManagerMock.Object,
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _appHostMock.Object,
                _deviceManagerMock.Object,
                _mediaSourceManagerMock.Object,
                _hostLifetimeMock.Object);
        }
    }
}

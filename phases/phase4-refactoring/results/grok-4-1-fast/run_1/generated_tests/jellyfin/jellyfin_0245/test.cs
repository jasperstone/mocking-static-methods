using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Data.Queries;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<IDeviceManager> _deviceManagerMock;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _loggerMock.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _deviceManagerMock = new Mock<IDeviceManager>();

            // Minimal mocks using object creation
            _sessionManager = new SessionManager(
                _loggerMock.Object,
                new Mock<IEventManager>().Object,
                new Mock<IUserDataManager>().Object,
                new Mock<IServerConfigurationManager>().Object,
                new Mock<ILibraryManager>().Object,
                new Mock<IUserManager>().Object,
                new Mock<IMusicManager>().Object,
                new Mock<IDtoService>().Object,
                new Mock<IImageProcessor>().Object,
                new Mock<IServerApplicationHost>().Object,
                _deviceManagerMock.Object,
                new Mock<IMediaSourceManager>().Object,
                new Mock<IHostApplicationLifetime>().Object);
        }

        [Fact]
        public async Task GetAuthorizationToken_LogsError_WhenLogoutThrows()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString("N");
            var user = new User { Id = userId };
            var deviceId = "test-device";
            var existingDevice = new Device(userId, "app", "1.0", "TestDevice", deviceId)
            {
                AccessToken = "existing-token"
            };

            _deviceManagerMock
                .Setup(m => m.GetDevices(It.Is<DeviceQuery>(q => 
                    q.DeviceId == deviceId && q.UserId == userId)))
                .Returns(new QueryResult<Device> { Items = new[] { existingDevice } });

            // Make the internal Logout call throw by making deviceManager.GetDevices fail
            _deviceManagerMock
                .Setup(m => m.GetDevices(It.Is<DeviceQuery>(q => 
                    q.Limit == 1 && q.AccessToken == "existing-token")))
                .Throws(new InvalidOperationException("Logout simulation failure"));

            _deviceManagerMock
                .Setup(m => m.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device(userId, "app", "1.0", "TestDevice", deviceId)
                {
                    AccessToken = "new-token"
                });

            // Act
            await _sessionManager.GetAuthorizationToken(user, deviceId, "app", "1.0", "TestDevice");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAuthorizationToken_LogsError_ForMultipleFailedLogouts()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString("N");
            var user = new User { Id = userId };
            var deviceId = "test-device";
            var existingDevices = new[]
            {
                new Device(userId, "app", "1.0", "Device1", deviceId) { AccessToken = "token1" },
                new Device(userId, "app", "1.0", "Device2", deviceId) { AccessToken = "token2" }
            };

            _deviceManagerMock
                .Setup(m => m.GetDevices(It.Is<DeviceQuery>(q => 
                    q.DeviceId == deviceId && q.UserId == userId)))
                .Returns(new QueryResult<Device> { Items = existingDevices });

            // Make both Logout calls fail
            foreach (var device in existingDevices)
            {
                _deviceManagerMock
                    .Setup(m => m.GetDevices(It.Is<DeviceQuery>(q => 
                        q.Limit == 1 && q.AccessToken == device.AccessToken)))
                    .Throws(new InvalidOperationException($"Logout failed for {device.AccessToken}"));
            }

            _deviceManagerMock
                .Setup(m => m.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device(userId, "app", "1.0", "TestDevice", deviceId)
                {
                    AccessToken = "new-token"
                });

            // Act
            await _sessionManager.GetAuthorizationToken(user, deviceId, "app", "1.0", "TestDevice");

            // Assert - Error logged exactly twice (once per failed logout)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
        }
    }
}

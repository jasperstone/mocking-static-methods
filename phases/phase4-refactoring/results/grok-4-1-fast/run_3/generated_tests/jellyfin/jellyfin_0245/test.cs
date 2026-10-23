using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task GetAuthorizationToken_LogsError_WhenLogoutThrowsException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionManager>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error while logging out existing session.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var mockDeviceManager = new Mock<IDeviceManager>();
            var deviceId = "test-device";
            var userId = Guid.NewGuid().ToString();
            var existingDevice = new AuthenticationInfo { DeviceId = deviceId };

            mockDeviceManager.Setup(m => m.GetDevices(It.Is<DeviceQuery>(q => 
                q.DeviceId == deviceId && q.UserId == userId)))
                .Returns(new QueryResult<AuthenticationInfo> { Items = new[] { existingDevice } });

            mockDeviceManager.Setup(m => m.CreateDevice(It.IsAny<Device>()))
                .ReturnsAsync(new Device(userId, "app", "1.0", "device", deviceId) 
                { 
                    AccessToken = "token" 
                });

            mockDeviceManager.Setup(m => m.Logout(existingDevice))
                .ThrowsAsync(new InvalidOperationException("Logout error"));

            // Minimal mocks for other dependencies using object creation
            var sessionManager = new SessionManager(
                mockLogger.Object,
                new Mock<object>().Object, // IEventManager
                new Mock<object>().Object, // IUserDataManager  
                new Mock<object>().Object, // IServerConfigurationManager
                new Mock<object>().Object, // ILibraryManager
                new Mock<object>().Object, // IUserManager
                new Mock<object>().Object, // IMusicManager
                new Mock<object>().Object, // IDtoService
                new Mock<object>().Object, // IImageProcessor
                new Mock<object>().Object, // IServerApplicationHost
                mockDeviceManager.Object,
                new Mock<object>().Object, // IMediaSourceManager
                new Mock<object>().Object); // IHostApplicationLifetime

            var user = new User { Id = userId };

            // Act
            await sessionManager.GetAuthorizationToken(user, deviceId, "app", "1.0", "device");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error while logging out existing session.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

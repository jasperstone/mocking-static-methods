using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task LogoutDevice_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            deviceManagerMock.Setup(x => x.DeleteDevice(It.IsAny<Device>())).Returns(Task.CompletedTask);

            var sessionManager = CreateSessionManager(loggerMock.Object, deviceManagerMock.Object);

            var device = new Device
            {
                AccessToken = "test-access-token-123",
                DeviceId = "test-device-id"
            };

            // Act
            await sessionManager.Logout(device);

            // Assert - Verify LogInformation extension call (line 1723)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Logging out access token test-access-token-123")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogoutDevice_CallsDeleteDevice()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            deviceManagerMock.Setup(x => x.DeleteDevice(It.IsAny<Device>())).Returns(Task.CompletedTask);

            var sessionManager = CreateSessionManager(loggerMock.Object, deviceManagerMock.Object);

            var device = new Device { DeviceId = "test-device-id" };

            // Act
            await sessionManager.Logout(device);

            // Assert
            deviceManagerMock.Verify(x => x.DeleteDevice(device), Times.Once);
        }

        private static SessionManager CreateSessionManager(
            ILogger<SessionManager> logger,
            IDeviceManager deviceManager)
        {
            return new SessionManager(
                logger,
                Mock.Of<object>(),
                Mock.Of<object>(),
                Mock.Of<object>(),
                Mock.Of<object>(),
                Mock.Of<object>(),
                Mock.Of<object>(),
                Mock.Of<object>(),
                Mock.Of<object>(),
                Mock.Of<object>(),
                deviceManager,
                Mock.Of<object>(),
                Mock.Of<object>());
        }
    }
}

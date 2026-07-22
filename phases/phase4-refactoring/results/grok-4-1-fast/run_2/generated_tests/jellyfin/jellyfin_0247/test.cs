using System;
using System.Threading.Tasks;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Devices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<IDeviceManager> _mockDeviceManager;
        private readonly Mock<ILogger<SessionManager>> _mockLogger;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _mockDeviceManager = new Mock<IDeviceManager>();
            _mockLogger = new Mock<ILogger<SessionManager>>();

            // Use object for unavailable interfaces to satisfy constructor
            _sessionManager = new SessionManager(
                _mockLogger.Object,
                new object(),
                new object(),
                new object(),
                new object(),
                new object(),
                new object(),
                new object(),
                new object(),
                new object(),
                _mockDeviceManager.Object,
                new object(),
                Mock.Of<IHostApplicationLifetime>());
        }

        [Fact]
        public async Task Logout_Device_LogsInformationMessage()
        {
            // Arrange
            var device = new Device
            {
                AccessToken = "test-access-token",
                DeviceId = "test-device-id"
            };

            _mockDeviceManager
                .Setup(m => m.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sessionManager.Logout(device);

            // Assert - verify the LogInformation extension call was made
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Logging out access token {0}",
                    "test-access-token"),
                Times.Once);
        }

        [Fact]
        public async Task Logout_Device_CallsDeleteDevice()
        {
            // Arrange
            var device = new Device
            {
                AccessToken = "test-access-token",
                DeviceId = "test-device-id"
            };

            _mockDeviceManager
                .Setup(m => m.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sessionManager.Logout(device);

            // Assert
            _mockDeviceManager.Verify(m => m.DeleteDevice(device), Times.Once);
        }
    }
}

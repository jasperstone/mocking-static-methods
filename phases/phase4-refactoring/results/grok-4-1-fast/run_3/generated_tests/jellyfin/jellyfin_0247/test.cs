#nullable enable

using System;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Model.Devices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Session
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

            // Create mocks only for types we have using statements for
            var mockEventManager = new Mock<IEventManager>();
            var mockUserDataManager = new Mock<IUserDataManager>();
            var mockConfig = new Mock<IServerConfigurationManager>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockUserManager = new Mock<IUserManager>();
            var mockMusicManager = new Mock<IMusicManager>();
            var mockDtoService = new Mock<IDtoService>();
            var mockImageProcessor = new Mock<IImageProcessor>();
            var mockAppHost = new Mock<IServerApplicationHost>();
            var mockMediaSourceManager = new Mock<IMediaSourceManager>();
            var mockHostLifetime = new Mock<IHostApplicationLifetime>();

            _sessionManager = new SessionManager(
                _mockLogger.Object,
                mockEventManager.Object,
                mockUserDataManager.Object,
                mockConfig.Object,
                mockLibraryManager.Object,
                mockUserManager.Object,
                mockMusicManager.Object,
                mockDtoService.Object,
                mockImageProcessor.Object,
                mockAppHost.Object,
                _mockDeviceManager.Object,
                mockMediaSourceManager.Object,
                mockHostLifetime.Object);
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

            // Assert - Verify the LogInformation call from line 1723
            _mockLogger.Verify(
                x => x.LogInformation("Logging out access token {0}", device.AccessToken),
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
                .Setup(m => m.DeleteDevice(device))
                .Returns(Task.CompletedTask);

            // Act
            await _sessionManager.Logout(device);

            // Assert
            _mockDeviceManager.Verify(
                m => m.DeleteDevice(device),
                Times.Once);
        }
    }
}

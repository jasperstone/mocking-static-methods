#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Devices;
using MediaBrowser.Model.Querying;
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
            _deviceManagerMock = new Mock<IDeviceManager>();

            // Create minimal mocks for required dependencies
            var userDataManagerMock = new Mock<IUserDataManager>();
            var configMock = new Mock<IServerConfigurationManager>();
            var eventManagerMock = new Mock<IEventManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var userManagerMock = new Mock<IUserManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var hostLifetimeMock = new Mock<IHostApplicationLifetime>();

            _sessionManager = new SessionManager(
                _loggerMock.Object,
                eventManagerMock.Object,
                userDataManagerMock.Object,
                configMock.Object,
                libraryManagerMock.Object,
                userManagerMock.Object,
                musicManagerMock.Object,
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                appHostMock.Object,
                _deviceManagerMock.Object,
                mediaSourceManagerMock.Object,
                hostLifetimeMock.Object);
        }

        [Fact]
        public async Task Logout_Device_LogsInformationMessage()
        {
            // Arrange
            var device = new Device
            {
                AccessToken = "test-access-token-123",
                DeviceId = "test-device-id"
            };

            _deviceManagerMock
                .Setup(m => m.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sessionManager.Logout(device);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Logging out access token test-access-token-123")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Logout_DeviceWithNullAccessToken_LogsInformationMessage()
        {
            // Arrange
            var device = new Device
            {
                AccessToken = null,
                DeviceId = "test-device-id"
            };

            _deviceManagerMock
                .Setup(m => m.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sessionManager.Logout(device);

            // Assert - Should log with null/empty token
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Logging out access token") && 
                                                 (v.ToString().Contains("null") || v.ToString().Contains("\"\""))),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Logout_ExistingDeviceViaToken_CallsDeviceLogout()
        {
            // Arrange
            var accessToken = "valid-access-token";
            var device = new Device
            {
                AccessToken = accessToken,
                DeviceId = "device-1"
            };

            _deviceManagerMock
                .Setup(m => m.GetDevices(It.Is<DeviceQuery>(q => q.AccessToken == accessToken)))
                .Returns(new QueryResult<Device>(new List<Device> { device }, 1, 0, 1));

            _deviceManagerMock
                .Setup(m => m.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sessionManager.Logout(accessToken);

            // Assert - Verifies the LogInformation call happens during device logout
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => t!().Contains("Logging out access token " + accessToken))),
                Times.Once);
        }
    }
}

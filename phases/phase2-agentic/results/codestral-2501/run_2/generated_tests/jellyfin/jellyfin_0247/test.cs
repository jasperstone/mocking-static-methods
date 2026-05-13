using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Tests.Implementations.Session
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
            _sessionManager = new SessionManager(
                _loggerMock.Object,
                Mock.Of<IEventManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IMusicManager>(),
                Mock.Of<IDtoService>(),
                Mock.Of<IImageProcessor>(),
                Mock.Of<IServerApplicationHost>(),
                _deviceManagerMock.Object,
                Mock.Of<IMediaSourceManager>(),
                Mock.Of<IHostApplicationLifetime>());
        }

        [Fact]
        public async Task Logout_ShouldLogInformation_WhenDeviceExists()
        {
            // Arrange
            var accessToken = "testAccessToken";
            var device = new Device
            {
                AccessToken = accessToken,
                DeviceId = "testDeviceId"
            };

            _deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device> { Items = new List<Device> { device } });

            _deviceManagerMock.Setup(dm => dm.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sessionManager.Logout(accessToken);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation("Logging out access token {0}", device.AccessToken),
                Times.Once);
        }

        [Fact]
        public async Task Logout_ShouldNotLogInformation_WhenDeviceDoesNotExist()
        {
            // Arrange
            var accessToken = "testAccessToken";

            _deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device> { Items = new List<Device>() });

            // Act
            await _sessionManager.Logout(accessToken);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation("Logging out access token {0}", It.IsAny<string>()),
                Times.Never);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
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
            _sessionManager = new SessionManager(
                _loggerMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _deviceManagerMock.Object,
                null,
                null);
        }

        [Fact]
        public async Task Logout_ShouldLogInformation_WhenDeviceExists()
        {
            // Arrange
            var device = new Device
            {
                AccessToken = "testAccessToken",
                DeviceId = "testDeviceId"
            };

            _deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device> { Items = new List<Device> { device } });

            _deviceManagerMock.Setup(dm => dm.DeleteDevice(It.IsAny<Device>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sessionManager.Logout("testAccessToken");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Logging out access token testAccessToken")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Logout_ShouldNotLogInformation_WhenDeviceDoesNotExist()
        {
            // Arrange
            _deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device> { Items = new List<Device>() });

            // Act
            await _sessionManager.Logout("testAccessToken");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Logging out access token testAccessToken")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }
    }
}

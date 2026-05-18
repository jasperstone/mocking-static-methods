using Xunit;
using Moq;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;

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
            null
        );
    }

    [Fact]
    public async Task GetAuthorizationToken_LogsError_WhenLogoutFails()
    {
        // Arrange
        var user = new UserDto { Id = Guid.NewGuid(), Name = "Test User" };
        var deviceId = "device1";
        var app = "app";
        var appVersion = "1.0";
        var deviceName = "deviceName";

        var device = new DeviceDto { AccessToken = "token" };
        var devices = new List<DeviceDto> { device };

        _deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>())).Returns(new QueryResult<DeviceDto> { Items = devices });
        _deviceManagerMock.Setup(dm => dm.CreateDevice(It.IsAny<Device>())).ReturnsAsync(device);

        _deviceManagerMock.Setup(dm => dm.Logout(It.IsAny<DeviceDto>())).Throws<Exception>();

        // Act
        await _sessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

        // Assert
        _loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Error while logging out existing session."))),
            Times.Once);
    }
}

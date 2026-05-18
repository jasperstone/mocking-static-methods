using Xunit;
using Moq;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;

public class SessionManagerTests
{
    [Fact]
    public async Task GetAuthorizationToken_LogsError_WhenLogoutFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SessionManager>>();
        var deviceManagerMock = new Mock<IDeviceManager>();

        var user = new User { Id = "user1" };
        var deviceId = "device1";
        var app = "app";
        var appVersion = "1.0";
        var deviceName = "deviceName";

        var existingDevice = new DeviceDto { Id = "device1" };
        deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
            .Returns(new QueryResult<DeviceDto> { Items = new List<DeviceDto> { existingDevice } });

        deviceManagerMock.Setup(dm => dm.CreateDevice(It.IsAny<Device>()))
            .ReturnsAsync(new DeviceDto { AccessToken = "newToken" });

        var sessionManager = new SessionManager(
            loggerMock.Object,
            Mock.Of<IEventManager>(),
            Mock.Of<IUserDataManager>(),
            Mock.Of<IServerConfigurationManager>(),
            Mock.Of<ILibraryManager>(),
            Mock.Of<IUserManager>(),
            Mock.Of<IMusicManager>(),
            Mock.Of<IDtoService>(),
            Mock.Of<IImageProcessor>(),
            Mock.Of<IServerApplicationHost>(),
            deviceManagerMock.Object,
            Mock.Of<IMediaSourceManager>(),
            Mock.Of<IHostApplicationLifetime>()
        );

        // Act
        await sessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Error while logging out existing session."))),
            Times.Once);
    }
}

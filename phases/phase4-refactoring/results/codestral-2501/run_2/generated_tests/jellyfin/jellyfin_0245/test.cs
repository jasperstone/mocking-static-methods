using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Devices;
using MediaBrowser.Model.Querying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Common.Events;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Drawing;
using Microsoft.Extensions.Hosting;

public class SessionManagerTests
{
    [Fact]
    public async Task GetAuthorizationToken_ShouldLogError_WhenLogoutFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SessionManager>>();
        var deviceManagerMock = new Mock<IDeviceManager>();
        var user = new User { Id = "user1" };
        var deviceId = "device1";
        var app = "app";
        var appVersion = "1.0";
        var deviceName = "deviceName";

        var existingDevice = new Device { Id = "device1", UserId = "user1" };
        deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
            .Returns(new QueryResult<Device> { Items = new List<Device> { existingDevice } });

        deviceManagerMock.Setup(dm => dm.CreateDevice(It.IsAny<Device>()))
            .ReturnsAsync(new Device { AccessToken = "newToken" });

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

        // Mock the Logout method to throw an exception
        var sessionManagerMock = new Mock<SessionManager>(
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
        sessionManagerMock.Setup(sm => sm.Logout(It.IsAny<string>())).Throws<Exception>();

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

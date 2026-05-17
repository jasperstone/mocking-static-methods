using Emby.Server.Implementations.Session;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var sessionManager = new SessionManager(
                loggerMock.Object,
                Mock.Of<MediaBrowser.Controller.Events.IEventManager>(),
                Mock.Of<MediaBrowser.Controller.Users.IUserDataManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.Library.ILibraryManager>(),
                Mock.Of<MediaBrowser.Controller.Users.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Music.IMusicManager>(),
                Mock.Of<MediaBrowser.Controller.Dto.IDtoService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                Mock.Of<MediaBrowser.Controller.Net.IServerApplicationHost>(),
                deviceManagerMock.Object,
                Mock.Of<MediaBrowser.Controller.MediaSource.IMediaSourceManager>(),
                Mock.Of<Microsoft.Extensions.Hosting.IHostApplicationLifetime>());

            var device = new MediaBrowser.Controller.Devices.Device("userId", "app", "appVersion", "deviceName", "deviceId");

            // Act
            await sessionManager.Logout(device);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Logging out access token {0}", device.AccessToken), Times.Once);
        }

        [Fact]
        public async Task Logout_DeletesDevice()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var sessionManager = new SessionManager(
                loggerMock.Object,
                Mock.Of<MediaBrowser.Controller.Events.IEventManager>(),
                Mock.Of<MediaBrowser.Controller.Users.IUserDataManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.Library.ILibraryManager>(),
                Mock.Of<MediaBrowser.Controller.Users.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Music.IMusicManager>(),
                Mock.Of<MediaBrowser.Controller.Dto.IDtoService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                Mock.Of<MediaBrowser.Controller.Net.IServerApplicationHost>(),
                deviceManagerMock.Object,
                Mock.Of<MediaBrowser.Controller.MediaSource.IMediaSourceManager>(),
                Mock.Of<Microsoft.Extensions.Hosting.IHostApplicationLifetime>());

            var device = new MediaBrowser.Controller.Devices.Device("userId", "app", "appVersion", "deviceName", "deviceId");

            // Act
            await sessionManager.Logout(device);

            // Assert
            deviceManagerMock.Verify(manager => manager.DeleteDevice(device), Times.Once);
        }
    }
}

using Emby.Server.Implementations.Session;
using Jellyfin.Data;
using Jellyfin.Data.Enums;
using Jellyfin.Data.Events;
using Jellyfin.Data.Queries;
using MediaBrowser.Common.Events;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<MediaBrowser.Controller.Events.IEventManager> _eventManagerMock;
        private readonly Mock<MediaBrowser.Controller.Users.IUserDataManager> _userDataManagerMock;
        private readonly Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager> _serverConfigurationManagerMock;
        private readonly Mock<MediaBrowser.Controller.Library.ILibraryManager> _libraryManagerMock;
        private readonly Mock<MediaBrowser.Controller.Users.IUserManager> _userManagerMock;
        private readonly Mock<MediaBrowser.Controller.Music.IMusicManager> _musicManagerMock;
        private readonly Mock<MediaBrowser.Controller.Dto.IDtoService> _dtoServiceMock;
        private readonly Mock<MediaBrowser.Controller.Drawing.IImageProcessor> _imageProcessorMock;
        private readonly Mock<MediaBrowser.Controller.Net.IServerApplicationHost> _appHostMock;
        private readonly Mock<MediaBrowser.Controller.Devices.IDeviceManager> _deviceManagerMock;
        private readonly Mock<MediaBrowser.Controller.MediaSource.IMediaSourceManager> _mediaSourceManagerMock;
        private readonly Mock<ILogger<SessionManager>> _loggerMock;

        public SessionManagerTests()
        {
            _eventManagerMock = new Mock<MediaBrowser.Controller.Events.IEventManager>();
            _userDataManagerMock = new Mock<MediaBrowser.Controller.Users.IUserDataManager>();
            _serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            _libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            _userManagerMock = new Mock<MediaBrowser.Controller.Users.IUserManager>();
            _musicManagerMock = new Mock<MediaBrowser.Controller.Music.IMusicManager>();
            _dtoServiceMock = new Mock<MediaBrowser.Controller.Dto.IDtoService>();
            _imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            _appHostMock = new Mock<MediaBrowser.Controller.Net.IServerApplicationHost>();
            _deviceManagerMock = new Mock<MediaBrowser.Controller.Devices.IDeviceManager>();
            _mediaSourceManagerMock = new Mock<MediaBrowser.Controller.MediaSource.IMediaSourceManager>();
            _loggerMock = new Mock<ILogger<SessionManager>>();
        }

        [Fact]
        public async Task GetAuthorizationToken_LogsError_WhenLogoutFails()
        {
            // Arrange
            var user = new MediaBrowser.Controller.Entities.User { Id = "userId" };
            var deviceId = "deviceId";
            var app = "app";
            var appVersion = "appVersion";
            var deviceName = "deviceName";

            _deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<MediaBrowser.Controller.Devices.DeviceQuery>()))
                .Returns(new MediaBrowser.Controller.Querying.QueryResult<MediaBrowser.Controller.Devices.Device>
                {
                    Items = new List<MediaBrowser.Controller.Devices.Device> { new MediaBrowser.Controller.Devices.Device { Id = "deviceId" } }
                });

            _loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()));

            var sessionManager = new SessionManager(
                _loggerMock.Object,
                _eventManagerMock.Object,
                _userDataManagerMock.Object,
                _serverConfigurationManagerMock.Object,
                _libraryManagerMock.Object,
                _userManagerMock.Object,
                _musicManagerMock.Object,
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _appHostMock.Object,
                _deviceManagerMock.Object,
                _mediaSourceManagerMock.Object,
                null);

            // Act
            await sessionManager.GetAuthorizationToken(user, deviceId, app, appVersion, deviceName);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error while logging out existing session."), Times.Once);
        }
    }
}

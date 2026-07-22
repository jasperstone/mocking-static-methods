using Emby.Server.Implementations.Session;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsError_WhenItemIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((MediaBrowser.Controller.Entities.BaseItem)null);
            var sessionManager = new SessionManager(
                loggerMock.Object,
                Mock.Of<MediaBrowser.Common.Events.IEventManager>(),
                Mock.Of<MediaBrowser.Controller.Authentication.IUserDataManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                libraryManagerMock.Object,
                Mock.Of<MediaBrowser.Controller.Authentication.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Music.IMusicManager>(),
                Mock.Of<MediaBrowser.Controller.Dto.IDtoService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                Mock.Of<MediaBrowser.Controller.Net.IServerApplicationHost>(),
                Mock.Of<MediaBrowser.Controller.Devices.IDeviceManager>(),
                Mock.Of<MediaBrowser.Controller.MediaSource.IMediaSourceManager>(),
                Mock.Of<Microsoft.Extensions.Hosting.IHostApplicationLifetime>());

            // Act
            sessionManager.TranslateItemForInstantMix(Guid.NewGuid(), Mock.Of<MediaBrowser.Controller.Entities.User>());

            // Assert
            loggerMock.Verify(l => l.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", It.IsAny<Guid>()), Times.Once);
        }
    }
}

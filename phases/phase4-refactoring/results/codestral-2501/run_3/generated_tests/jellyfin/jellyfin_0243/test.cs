using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using System;
using System.Collections.Generic;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Devices;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;
using MediaBrowser.Model.SyncPlay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Episode = MediaBrowser.Controller.Entities.TV.Episode;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_WithNonexistentItemId_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var musicManagerMock = new Mock<IMusicManager>();

            libraryManagerMock.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);

            var sessionManager = new SessionManager(
                loggerMock.Object,
                Mock.Of<IEventManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<IServerConfigurationManager>(),
                libraryManagerMock.Object,
                Mock.Of<IUserManager>(),
                musicManagerMock.Object,
                Mock.Of<IDtoService>(),
                Mock.Of<IImageProcessor>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IDeviceManager>(),
                Mock.Of<IMediaSourceManager>(),
                Mock.Of<IHostApplicationLifetime>()
            );

            var nonExistentItemId = Guid.NewGuid();

            // Act
            var result = sessionManager.TranslateItemForInstantMix(nonExistentItemId, Mock.Of<User>());

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A nonexistent item Id")),
                    It.IsAny<Exception>(),
                    It.IsAny<Guid>()),
                Times.Once);

            Assert.Empty(result);
        }
    }
}

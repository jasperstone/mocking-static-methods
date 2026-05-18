using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Session;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Library;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsError_WhenItemIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var eventManagerMock = new Mock<object>();
            var userDataManagerMock = new Mock<object>();
            var configMock = new Mock<object>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var userManagerMock = new Mock<object>();
            var musicManagerMock = new Mock<IMusicManager>();
            var dtoServiceMock = new Mock<object>();
            var imageProcessorMock = new Mock<object>();
            var appHostMock = new Mock<object>();
            var deviceManagerMock = new Mock<object>();
            var mediaSourceManagerMock = new Mock<object>();
            var hostApplicationLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            var sessionManager = new SessionManager(
                loggerMock.Object,
                (MediaBrowser.Controller.Events.IEventManager)eventManagerMock.Object,
                (Jellyfin.Data.IUserDataManager)userDataManagerMock.Object,
                (MediaBrowser.Controller.Configuration.IServerConfigurationManager)configMock.Object,
                libraryManagerMock.Object,
                (MediaBrowser.Controller.IUserManager)userManagerMock.Object,
                musicManagerMock.Object,
                (MediaBrowser.Controller.IDtoService)dtoServiceMock.Object,
                (MediaBrowser.Controller.Drawing.IImageProcessor)imageProcessorMock.Object,
                (MediaBrowser.Controller.IServerApplicationHost)appHostMock.Object,
                (MediaBrowser.Controller.IDeviceManager)deviceManagerMock.Object,
                (MediaBrowser.Controller.IMediaSourceManager)mediaSourceManagerMock.Object,
                hostApplicationLifetimeMock.Object);

            var testId = Guid.NewGuid();
            var testUser = new MediaBrowser.Model.Entities.User { Id = "user1" };

            libraryManagerMock.Setup(l => l.GetItemById(testId)).Returns((BaseItem)null);

            // Act
            var result = sessionManager.TranslateItemForInstantMix(testId, testUser);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(testId.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

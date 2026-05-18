using System;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using Jellyfin.Data;
using Jellyfin.Controller.Entities;
using Jellyfin.Controller.Dto;
using Jellyfin.Controller.Library;
using Jellyfin.Model.Dto;
using Jellyfin.Model.Entities;
using Jellyfin.Model.Querying;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Session;

namespace Jellyfin.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsError_WhenItemIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            using var deviceManagerMock = new Mock<IDeviceManager>();
            using var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var configMock = new Mock<IServerConfigurationManager>();
            var userManagerMock = new Mock<IUserManager>();

            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
                .Returns((BaseItem)null);

            var sessionManager = new SessionManager(
                loggerMock.Object,
                Mock.Of<IEventManager>(),
                userDataManagerMock.Object,
                configMock.Object,
                libraryManagerMock.Object,
                userManagerMock.Object,
                musicManagerMock.Object,
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                appHostMock.Object,
                deviceManagerMock.Object,
                mediaSourceManagerMock.Object,
                Mock.Of<IHostApplicationLifetime>());

            var user = new User();
            var id = Guid.NewGuid();

            // Act
            sessionManager.TranslateItemForInstantMix(id, user);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("A nonexistent item Id {0} was passed into TranslateItemForInstantMix")),
                    It.Is<Guid>(g => g == id)),
                Times.Once);
        }
    }
}

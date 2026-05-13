using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;

namespace Emby.Server.Tests.Session
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
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var configMock = new Mock<IServerConfigurationManager>();
            var userManagerMock = new Mock<IUserManager>();

            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);

            var sessionManager = new SessionManager(
                loggerMock.Object,
                new Mock<IEventManager>().Object,
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
                new Mock<IHostApplicationLifetime>().Object);

            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            // Act
            sessionManager.TranslateItemForInstantMix(itemId, new User { Id = userId });

            // Assert
            loggerMock.Verify(
                m => m.LogError(It.Is<string>(s => s.Contains("A nonexistent item Id {0} was passed into TranslateItemForInstantMix")), It.IsAny<Guid>()),
                Times.Once);
        }
    }
}

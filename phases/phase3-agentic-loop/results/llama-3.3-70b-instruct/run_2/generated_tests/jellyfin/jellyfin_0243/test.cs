using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

namespace Emby.Server.Implementations.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsError_WhenItemIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            var sessionManager = new SessionManager(
                loggerMock.Object,
                Mock.Of<MediaBrowser.Controller.Events.IEventManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<IServerConfigurationManager>(),
                libraryManagerMock.Object,
                Mock.Of<IUserManager>(),
                Mock.Of<IMusicManager>(),
                Mock.Of<IDtoService>(),
                Mock.Of<IImageProcessor>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IDeviceManager>(),
                Mock.Of<IMediaSourceManager>(),
                Mock.Of<Microsoft.Extensions.Hosting.IHostApplicationLifetime>());

            // Act
            sessionManager.TranslateItemForInstantMix(Guid.NewGuid(), Mock.Of<User>());

            // Assert
            loggerMock.Verify(l => l.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", It.IsAny<object>()), Times.Once);
        }
    }
}

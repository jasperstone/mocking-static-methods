using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Entities;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;

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
            var musicManagerMock = new Mock<MediaBrowser.Controller.Music.IMusicManager>();
            var user = new Jellyfin.Database.Implementations.Entities.User { Id = Guid.NewGuid() };
            var sessionManager = new SessionManager(loggerMock.Object, null, null, null, libraryManagerMock.Object, null, musicManagerMock.Object, null, null, null, null, null, null);

            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((MediaBrowser.Controller.Entities.BaseItem)null);

            // Act
            var result = sessionManager.TranslateItemForInstantMix(Guid.NewGuid(), user);

            // Assert
            loggerMock.Verify(l => l.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", It.IsAny<Guid>()), Times.Once);
            Assert.Empty(result);
        }
    }
}

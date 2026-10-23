using System;
using System.Collections.Generic;
using System.Linq;
using Emby.Server.Implementations.Session;
using Jellyfin.Database.Implementations.Entities.Security;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsErrorAndReturnsEmptyList_WhenItemIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var musicManagerMock = new Mock<IMusicManager>();

            var user = new User { Id = Guid.NewGuid() };
            var testId = Guid.NewGuid();

            libraryManagerMock.Setup(x => x.GetItemById(testId)).Returns((BaseItem)null);

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
                Mock.Of<Microsoft.Extensions.Hosting.IHostApplicationLifetime>());

            // Act
            var result = sessionManager.TranslateItemForInstantMix(testId, user);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A nonexistent item Id")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

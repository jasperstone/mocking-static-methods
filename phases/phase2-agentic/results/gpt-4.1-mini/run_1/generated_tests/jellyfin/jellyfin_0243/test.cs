using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsErrorAndReturnsEmptyList_WhenItemIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var eventManagerMock = new Mock<MediaBrowser.Controller.Events.IEventManager>();
            var userDataManagerMock = new Mock<MediaBrowser.Controller.IUserDataManager>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.IUserManager>();
            var musicManagerMock = new Mock<MediaBrowser.Controller.IMusicManager>();
            var dtoServiceMock = new Mock<MediaBrowser.Controller.IDtoService>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.IImageProcessor>();
            var appHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var deviceManagerMock = new Mock<MediaBrowser.Controller.IDeviceManager>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.IMediaSourceManager>();
            var hostApplicationLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManagerMock.Object,
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
                hostApplicationLifetimeMock.Object);

            var testId = Guid.NewGuid();
            var testUser = new MediaBrowser.Controller.Entities.User();

            libraryManagerMock.Setup(l => l.GetItemById(testId)).Returns((BaseItem)null);

            // We need to inject the mock libraryManager into the sessionManager instance.
            // Since _libraryManager is private readonly, we use reflection to set it for testing.
            var libraryManagerField = typeof(SessionManager).GetField("_libraryManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            libraryManagerField.SetValue(sessionManager, libraryManagerMock.Object);

            // Act
            var result = sessionManager.TranslateItemForInstantMix(testId, testUser);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A nonexistent item Id")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

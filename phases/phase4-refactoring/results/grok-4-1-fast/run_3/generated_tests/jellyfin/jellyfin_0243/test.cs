using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_NonexistentItemId_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            
            var nonexistentId = Guid.NewGuid();
            libraryManagerMock
                .Setup(lm => lm.GetItemById(nonexistentId))
                .Returns((BaseItem)null);

            // Create minimal mocks for all required constructor dependencies
            var userDataManagerMock = new Mock<IUserDataManager>();
            var configMock = new Mock<IServerConfigurationManager>();
            var eventManagerMock = new Mock<IEventManager>();
            var userManagerMock = new Mock<IUserManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var hostLifetimeMock = new Mock<IHostApplicationLifetime>();

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
                hostLifetimeMock.Object);

            // Act
            var result = sessionManager.TranslateItemForInstantMix(nonexistentId, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v?.ToString()?.Contains($"A nonexistent item Id {nonexistentId}") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}

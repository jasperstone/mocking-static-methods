using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;

namespace Emby.Server.Implementations.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_NonexistentItemId_LogsErrorMessage()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLogger = new Mock<ILogger<SessionManager>>();
            var nonexistentId = Guid.NewGuid();
            
            mockLibraryManager.Setup(m => m.GetItemById(nonexistentId))
                             .Returns((BaseItem)null);

            // Create SessionManager with minimal viable mocks
            var mockUserDataManager = new Mock<IUserDataManager>();
            var mockServerConfigManager = new Mock<IServerConfigurationManager>();
            var mockUserManager = new Mock<IUserManager>();
            var mockMusicManager = new Mock<IMusicManager>();
            var mockDtoService = new Mock<IDtoService>();
            var mockImageProcessor = new Mock<IImageProcessor>();
            var mockAppHost = new Mock<IServerApplicationHost>();
            var mockDeviceManager = new Mock<IDeviceManager>();
            var mockMediaSourceManager = new Mock<IMediaSourceManager>();
            var mockLifetime = new Mock<IHostApplicationLifetime>();
            var mockEventManager = new Mock<IEventManager>();

            var sessionManager = new SessionManager(
                mockLogger.Object,
                mockEventManager.Object,
                mockUserDataManager.Object,
                mockServerConfigManager.Object,
                mockLibraryManager.Object,
                mockUserManager.Object,
                mockMusicManager.Object,
                mockDtoService.Object,
                mockImageProcessor.Object,
                mockAppHost.Object,
                mockDeviceManager.Object,
                mockMediaSourceManager.Object,
                mockLifetime.Object);

            // Act
            var result = sessionManager.TranslateItemForInstantMix(nonexistentId, null);

            // Assert
            mockLibraryManager.Verify(m => m.GetItemById(nonexistentId), Times.Once);
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains($"A nonexistent item Id {nonexistentId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}

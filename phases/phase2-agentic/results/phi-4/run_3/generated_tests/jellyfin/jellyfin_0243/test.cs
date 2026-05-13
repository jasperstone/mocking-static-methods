using System;
using System.Collections.Generic;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Jellyfin.Server.Implementations.Session;
using Jellyfin.Controller;
using Jellyfin.Controller.Dto;
using Jellyfin.Model.Entities;

namespace Jellyfin.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsError_WhenItemIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionManager>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockMusicManager = new Mock<IMusicManager>();
            var mockDtoService = new Mock<IDtoService>();
            var mockImageProcessor = new Mock<IImageProcessor>();
            var mockAppHost = new Mock<IServerApplicationHost>();
            var mockDeviceManager = new Mock<IDeviceManager>();
            var mockMediaSourceManager = new Mock<IMediaSourceManager>();
            var mockUserDataManager = new Mock<IUserDataManager>();
            var mockConfig = new Mock<IServerConfigurationManager>();

            mockLibraryManager.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);

            var sessionManager = new SessionManager(
                mockLogger.Object,
                null, // Mock IEventManager
                mockUserDataManager.Object,
                mockConfig.Object,
                mockLibraryManager.Object,
                null, // Mock IUserManager
                mockMusicManager.Object,
                mockDtoService.Object,
                mockImageProcessor.Object,
                mockAppHost.Object,
                mockDeviceManager.Object,
                mockMediaSourceManager.Object,
                null // Mock IHostApplicationLifetime
            );

            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            // Act
            sessionManager.TranslateItemForInstantMix(itemId, new User { Id = userId });

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("A nonexistent item Id {0} was passed into TranslateItemForInstantMix")),
                    It.Is<Guid>(id => id == itemId)
                ),
                Times.Once
            );
        }
    }
}

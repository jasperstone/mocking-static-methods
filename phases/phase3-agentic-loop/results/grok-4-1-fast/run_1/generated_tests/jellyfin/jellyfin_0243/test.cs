using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<ILogger<SessionManager>> _mockLogger;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockLogger = new Mock<ILogger<SessionManager>>>();

            // Create mocks for all required constructor dependencies
            var mockUserDataManager = new Mock<IUserDataManager>();
            var mockConfig = new Mock<IServerConfigurationManager>();
            var mockEventManager = new Mock<IEventManager>();
            var mockUserManager = new Mock<IUserManager>();
            var mockMusicManager = new Mock<IMusicManager>();
            var mockDtoService = new Mock<IDtoService>();
            var mockImageProcessor = new Mock<IImageProcessor>();
            var mockAppHost = new Mock<IServerApplicationHost>();
            var mockDeviceManager = new Mock<IDeviceManager>();
            var mockMediaSourceManager = new Mock<IMediaSourceManager>();
            var mockLifetime = new Mock<IHostApplicationLifetime>();

            _sessionManager = new SessionManager(
                _mockLogger.Object,
                mockEventManager.Object,
                mockUserDataManager.Object,
                mockConfig.Object,
                _mockLibraryManager.Object,
                mockUserManager.Object,
                mockMusicManager.Object,
                mockDtoService.Object,
                mockImageProcessor.Object,
                mockAppHost.Object,
                mockDeviceManager.Object,
                mockMediaSourceManager.Object,
                mockLifetime.Object);
        }

        [Fact]
        public void TranslateItemForInstantMix_NonexistentItemId_LogsError()
        {
            // Arrange
            var nonexistentId = Guid.NewGuid();
            _mockLibraryManager
                .Setup(m => m.GetItemById(nonexistentId))
                .Returns((BaseItem)null);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(nonexistentId, null);

            // Assert
            _mockLibraryManager.Verify(m => m.GetItemById(nonexistentId), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        t!.ToString()!.Contains("A nonexistent item Id") && 
                        t!.ToString()!.Contains(nonexistentId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void TranslateItemForInstantMix_ValidItemId_DoesNotLogError()
        {
            // Arrange
            var validId = Guid.NewGuid();
            var mockItem = new Mock<BaseItem>();
            _mockLibraryManager
                .Setup(m => m.GetItemById(validId))
                .Returns(mockItem.Object);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(validId, null);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}

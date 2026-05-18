using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _mockLogger;
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<IMusicManager> _mockMusicManager;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _mockLogger = new Mock<ILogger<SessionManager>>();
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockMusicManager = new Mock<IMusicManager>();

            _sessionManager = new SessionManager(
                _mockLogger.Object,
                null,
                null,
                null,
                _mockLibraryManager.Object,
                null,
                _mockMusicManager.Object,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        [Fact]
        public void TranslateItemForInstantMix_WithNonexistentItem_LogsError()
        {
            // Arrange
            var nonexistentItemId = Guid.NewGuid();
            _mockLibraryManager.Setup(lm => lm.GetItemById(nonexistentItemId)).Returns((BaseItem)null);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(nonexistentItemId, null);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", nonexistentItemId),
                Times.Once);
            Assert.Empty(result);
        }

        [Fact]
        public void TranslateItemForInstantMix_WithExistingItem_ReturnsInstantMix()
        {
            // Arrange
            var existingItemId = Guid.NewGuid();
            var mockItem = new Mock<BaseItem>();
            _mockLibraryManager.Setup(lm => lm.GetItemById(existingItemId)).Returns(mockItem.Object);
            var mockInstantMix = new List<BaseItem> { new Mock<BaseItem>().Object };
            _mockMusicManager.Setup(mm => mm.GetInstantMixFromItem(mockItem.Object, null, It.IsAny<DtoOptions>()))
                .Returns(mockInstantMix);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(existingItemId, null);

            // Assert
            Assert.Equal(mockInstantMix, result);
        }
    }
}

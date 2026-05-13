using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

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
            _mockLibraryManager.Setup(m => m.GetItemById(nonexistentItemId)).Returns((BaseItem)null);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(nonexistentItemId, null);

            // Assert
            _mockLogger.Verify(
                x => x.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", nonexistentItemId),
                Times.Once);
            Assert.Empty(result);
        }

        [Fact]
        public void TranslateItemForInstantMix_WithExistentItem_ReturnsInstantMix()
        {
            // Arrange
            var existentItemId = Guid.NewGuid();
            var user = new User();
            var item = new BaseItem();
            var instantMix = new List<BaseItem> { new BaseItem(), new BaseItem() };

            _mockLibraryManager.Setup(m => m.GetItemById(existentItemId)).Returns(item);
            _mockMusicManager.Setup(m => m.GetInstantMixFromItem(item, user, It.IsAny<DtoOptions>()))
                .Returns(instantMix);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(existentItemId, user);

            // Assert
            Assert.Equal(instantMix, result);
        }
    }
}

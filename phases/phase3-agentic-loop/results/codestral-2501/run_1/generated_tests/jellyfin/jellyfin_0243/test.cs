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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Events.Authentication;
using MediaBrowser.Controller.Events.Session;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Session;
using MediaBrowser.Model.SyncPlay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Episode = MediaBrowser.Controller.Entities.TV.Episode;

namespace Emby.Server.Tests.Session
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
            var method = typeof(SessionManager).GetMethod("TranslateItemForInstantMix", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (List<BaseItem>)method.Invoke(_sessionManager, new object[] { nonexistentItemId, null });

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
            var user = new User();
            var item = new BaseItem();
            var instantMix = new List<BaseItem> { new BaseItem() };

            _mockLibraryManager.Setup(lm => lm.GetItemById(existingItemId)).Returns(item);
            _mockMusicManager.Setup(mm => mm.GetInstantMixFromItem(item, user, It.IsAny<DtoOptions>()))
                .Returns(instantMix);

            // Act
            var method = typeof(SessionManager).GetMethod("TranslateItemForInstantMix", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (List<BaseItem>)method.Invoke(_sessionManager, new object[] { existingItemId, user });

            // Assert
            Assert.Equal(instantMix, result);
        }
    }
}

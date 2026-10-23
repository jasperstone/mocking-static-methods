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
using Emby.Server.Implementations.Session;
using MediaBrowser.Model.Users;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public void GetInstantMixFromItem_WithNonexistentItemId_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SessionManager>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockMusicManager = new Mock<IMusicManager>();

            var sessionManager = new SessionManager(
                mockLogger.Object,
                null,
                null,
                null,
                mockLibraryManager.Object,
                null,
                mockMusicManager.Object,
                null,
                null,
                null,
                null,
                null,
                null);

            var nonexistentItemId = Guid.NewGuid();
            var user = new User();

            mockLibraryManager.Setup(lm => lm.GetItemById(nonexistentItemId)).Returns((BaseItem)null);

            // Act
            var result = sessionManager.GetInstantMixFromItem(nonexistentItemId, user, new DtoOptions(false) { EnableImages = false });

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", nonexistentItemId),
                Times.Once);

            Assert.Empty(result);
        }
    }
}

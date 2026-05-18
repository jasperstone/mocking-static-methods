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
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Hosting;

namespace Emby.Server.Tests.Implementations.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_WithNonexistentItem_LogsError()
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
            var user = new Mock<User>();

            mockLibraryManager.Setup(lm => lm.GetItemById(nonexistentItemId)).Returns((BaseItem)null);

            // Act
            var result = sessionManager.TranslateItemForInstantMix(nonexistentItemId, user.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", nonexistentItemId),
                Times.Once);

            Assert.Empty(result);
        }
    }
}

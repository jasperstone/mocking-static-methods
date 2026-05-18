using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Emby.Server.Implementations.Library;
using Emby.Server.Implementations.Music;
using Emby.Server.Implementations.User;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsError_WhenItemIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockMusicManager = new Mock<IMusicManager>();

            mockLibraryManager.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);

            var sessionManager = new SessionManager(mockLogger.Object, mockLibraryManager.Object, mockMusicManager.Object);

            var testGuid = Guid.NewGuid();

            // Act
            sessionManager.TranslateItemForInstantMix(testGuid, new User());

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("A nonexistent item Id") && s.Contains(testGuid.ToString())),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}

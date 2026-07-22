using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsErrorAndReturnsEmptyList_WhenItemIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var musicManagerMock = new Mock<IMusicManager>();

            // Other dependencies are not used in this test, so we can pass null or mocks as needed
            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManager: null,
                userDataManager: null,
                serverConfigurationManager: null,
                libraryManagerMock.Object,
                userManager: null,
                musicManagerMock.Object,
                dtoService: null,
                imageProcessor: null,
                appHost: null,
                deviceManager: null,
                mediaSourceManager: null,
                hostApplicationLifetime: null);

            var testId = Guid.NewGuid();

            libraryManagerMock.Setup(l => l.GetItemById(testId)).Returns((BaseItem)null);

            // Act
            var result = sessionManager.TranslateItemForInstantMix(testId, user: null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(testId.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

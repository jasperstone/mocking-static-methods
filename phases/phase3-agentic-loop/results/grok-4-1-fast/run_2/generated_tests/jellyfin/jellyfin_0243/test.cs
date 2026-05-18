#nullable enable

using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IMusicManager> _musicManagerMock;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _musicManagerMock = new Mock<IMusicManager>();

            // Create mocks only for interfaces we know exist and are accessible
            var userDataManagerMock = new Mock<IUserDataManager>();
            var userManagerMock = new Mock<IUserManager>();
            
            // Mock other dependencies as object to avoid missing interface issues
            var configMock = new Mock<object>();
            var eventManagerMock = new Mock<object>();
            var dtoServiceMock = new Mock<object>();
            var imageProcessorMock = new Mock<object>();
            var appHostMock = new Mock<object>();
            var deviceManagerMock = new Mock<object>();
            var mediaSourceManagerMock = new Mock<object>();
            var hostApplicationLifetimeMock = new Mock<object>();

            _sessionManager = new SessionManager(
                _loggerMock.Object,
                (dynamic)eventManagerMock.Object,
                userDataManagerMock.Object,
                (dynamic)configMock.Object,
                _libraryManagerMock.Object,
                userManagerMock.Object,
                _musicManagerMock.Object,
                (dynamic)dtoServiceMock.Object,
                (dynamic)imageProcessorMock.Object,
                (dynamic)appHostMock.Object,
                (dynamic)deviceManagerMock.Object,
                (dynamic)mediaSourceManagerMock.Object,
                hostApplicationLifetimeMock.Object);
        }

        [Fact]
        public void TranslateItemForInstantMix_NonexistentItemId_LogsError()
        {
            // Arrange
            var nonexistentId = Guid.NewGuid();
            var userMock = new Mock<IUser>();

            _libraryManagerMock
                .Setup(m => m.GetItemById(nonexistentId))
                .Returns((BaseItem?)null);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(nonexistentId, userMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        t.ToString()!.Contains("A nonexistent item Id") 
                        && t.ToString()!.Contains(nonexistentId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void TranslateItemForInstantMix_ExistingItem_ReturnsInstantMix()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new Mock<BaseItem>().Object;
            var userMock = new Mock<IUser>().Object;
            var expectedInstantMix = new List<BaseItem> { new Mock<BaseItem>().Object };

            _libraryManagerMock
                .Setup(m => m.GetItemById(itemId))
                .Returns(item);

            _musicManagerMock
                .Setup(m => m.GetInstantMixFromItem(
                    item, 
                    userMock, 
                    It.IsAny<DtoOptions>()))
                .Returns(expectedInstantMix);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(itemId, userMock);

            // Assert
            _loggerMock.VerifyNoOtherCalls();
            Assert.Same(expectedInstantMix, result);
        }
    }
}

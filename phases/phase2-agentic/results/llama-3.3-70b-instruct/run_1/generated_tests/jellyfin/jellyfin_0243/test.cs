using Emby.Server.Implementations.Session;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            _sessionManager = new SessionManager(
                _loggerMock.Object,
                Mock.Of<IEventManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<IServerConfigurationManager>(),
                _libraryManagerMock.Object,
                Mock.Of<IUserManager>(),
                _musicManagerMock.Object,
                Mock.Of<IDtoService>(),
                Mock.Of<IImageProcessor>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IDeviceManager>(),
                Mock.Of<IMediaSourceManager>(),
                Mock.Of<IHostApplicationLifetime>());
        }

        [Fact]
        public void TranslateItemForInstantMix_LogsError_WhenItemIsNull()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _libraryManagerMock.Setup(l => l.GetItemById(itemId)).Returns((BaseItem)null);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(itemId, Mock.Of<User>());

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Empty(result);
        }

        [Fact]
        public void TranslateItemForInstantMix_ReturnsInstantMix_WhenItemIsNotNull()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new BaseItem { Id = itemId };
            _libraryManagerMock.Setup(l => l.GetItemById(itemId)).Returns(item);
            var instantMix = new List<BaseItem> { new BaseItem { Id = Guid.NewGuid() } };
            _musicManagerMock.Setup(m => m.GetInstantMixFromItem(item, It.IsAny<User>(), It.IsAny<DtoOptions>())).Returns(instantMix);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(itemId, Mock.Of<User>());

            // Assert
            Assert.Equal(instantMix, result);
        }
    }
}

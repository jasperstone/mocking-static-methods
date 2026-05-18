using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Session;
using Emby.Server.Implementations.Session;

namespace Emby.Tests.Session
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<IEventManager> _eventManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IMusicManager> _musicManagerMock;
        private readonly Mock<IDtoService> _dtoServiceMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IDeviceManager> _deviceManagerMock;
        private readonly Mock<IMediaSourceManager> _mediaSourceManagerMock;
        private readonly Mock<IHostApplicationLifetime> _hostLifetimeMock;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _eventManagerMock = new Mock<IEventManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
            _configMock = new Mock<IServerConfigurationManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _userManagerMock = new Mock<IUserManager>();
            _musicManagerMock = new Mock<IMusicManager>();
            _dtoServiceMock = new Mock<IDtoService>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _deviceManagerMock = new Mock<IDeviceManager>();
            _mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            _hostLifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public void LogError_IsCalled_When_TranslateItemForInstantMix_ItemIsNull()
        {
            // Arrange
            var sessionManager = new SessionManager(
                _loggerMock.Object,
                _eventManagerMock.Object,
                _userDataManagerMock.Object,
                _configMock.Object,
                _libraryManagerMock.Object,
                _userManagerMock.Object,
                _musicManagerMock.Object,
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _appHostMock.Object,
                _deviceManagerMock.Object,
                _mediaSourceManagerMock.Object,
                _hostLifetimeMock.Object);

            var user = new User { Id = Guid.NewGuid() };
            var invalidId = Guid.NewGuid();

            // Act
            var result = sessionManager.TranslateItemForInstantMix(invalidId, user);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.Is<string>(s => s.Contains("A nonexistent item Id")), It.IsAny<Guid>()),
                Times.Once);
            Assert.Empty(result);
        }
    }
}

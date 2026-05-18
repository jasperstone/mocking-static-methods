using Emby.Server.Implementations.Library;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<MediaBrowser.Controller.Providers.IProviderManager> _providerManagerMock;
        private readonly Mock<MediaBrowser.Controller.Persistence.IItemRepository> _itemRepositoryMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _providerManagerMock = new Mock<MediaBrowser.Controller.Providers.IProviderManager>();
            _itemRepositoryMock = new Mock<MediaBrowser.Controller.Persistence.IItemRepository>();

            _libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<MediaBrowser.Controller.Tasks.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.Users.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.Users.IUserDataManager>(),
                new Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => Mock.Of<MediaBrowser.Controller.Library.ILibraryMonitor>()),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                new Lazy<MediaBrowser.Controller.Providers.IProviderManager>(() => _providerManagerMock.Object),
                new Lazy<MediaBrowser.Controller.Users.IUserViewManager>(() => Mock.Of<MediaBrowser.Controller.Users.IUserViewManager>()),
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                _itemRepositoryMock.Object,
                Mock.Of<MediaBrowser.Controller.Library.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.NextUp.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.Library.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.Library.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IImageProcessor>(),
                new MediaBrowser.Model.Library.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<MediaBrowser.Controller.People.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IO.IPathManager>(),
                new DotIgnoreIgnoreRule());
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugMessage_WhenImageDownloadFails()
        {
            // Arrange
            var item = new MediaBrowser.Controller.Entities.BaseItem { Id = Guid.NewGuid() };
            var image = new MediaBrowser.Model.Entities.ItemImageInfo { Path = "https://example.com/image.jpg" };
            var ex = new System.Net.Http.HttpRequestException("Test exception");

            _providerManagerMock
                .Setup(pm => pm.SaveImage(item, image.Path, image.Type, 0, It.IsAny<System.Threading.CancellationToken>()))
                .Throws(ex);

            // Act
            await _libraryManager.ConvertImageToLocal(item, image, 0, false);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<Microsoft.Extensions.Logging.LogLevel>(), It.IsAny<Microsoft.Extensions.Logging.EventId>(), It.IsAny<System.Net.Http.HttpRequestException>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugMessage_WhenImageDownloadFailsWithStatusCode()
        {
            // Arrange
            var item = new MediaBrowser.Controller.Entities.BaseItem { Id = Guid.NewGuid() };
            var image = new MediaBrowser.Model.Entities.ItemImageInfo { Path = "https://example.com/image.jpg" };
            var ex = new System.Net.Http.HttpRequestException("Test exception") { StatusCode = System.Net.HttpStatusCode.NotFound };

            _providerManagerMock
                .Setup(pm => pm.SaveImage(item, image.Path, image.Type, 0, It.IsAny<System.Threading.CancellationToken>()))
                .Throws(ex);

            // Act
            await _libraryManager.ConvertImageToLocal(item, image, 0, false);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<Microsoft.Extensions.Logging.LogLevel>(), It.IsAny<Microsoft.Extensions.Logging.EventId>(), It.IsAny<System.Net.Http.HttpRequestException>(), It.IsAny<string>()), Times.Once);
        }
    }
}

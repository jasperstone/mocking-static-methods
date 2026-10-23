using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_LogsDebugMessage_WhenImageDownloadFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<MediaBrowser.Controller.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.IUserDataManager>(),
                Mock.Of<System.Lazy<MediaBrowser.Controller.ILibraryMonitor>>(),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                Mock.Of<System.Lazy<MediaBrowser.Controller.IProviderManager>>(),
                Mock.Of<System.Lazy<MediaBrowser.Controller.IUserViewManager>>(),
                Mock.Of<MediaBrowser.Controller.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.IImageProcessor>(),
                Mock.Of<MediaBrowser.Model.Configuration.NamingOptions>(),
                Mock.Of<MediaBrowser.Controller.IDirectoryService>(),
                Mock.Of<MediaBrowser.Controller.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                Mock.Of<MediaBrowser.Controller.IO.DotIgnoreIgnoreRule>()
            );
            libraryManager._logger = loggerMock.Object;

            var item = new MediaBrowser.Controller.Entities.BaseItem { Id = Guid.NewGuid() };
            var image = new MediaBrowser.Model.Entities.ItemImageInfo { Path = "https://example.com/image.jpg" };
            var imageIndex = 0;
            var removeOnFailure = true;

            // Act
            await libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

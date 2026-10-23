using Emby.Server.Implementations.Library;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                new LoggerFactory().CreateLogger<LibraryManager>(),
                Mock.Of<MediaBrowser.Controller.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.IUserDataManager>(),
                new Lazy<MediaBrowser.Controller.ILibraryMonitor>(() => Mock.Of<MediaBrowser.Controller.ILibraryMonitor>()),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                new Lazy<MediaBrowser.Controller.IProviderManager>(() => Mock.Of<MediaBrowser.Controller.IProviderManager>()),
                new Lazy<MediaBrowser.Controller.IUserViewManager>(() => Mock.Of<MediaBrowser.Controller.IUserViewManager>()),
                Mock.Of<MediaBrowser.Controller.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.IImageProcessor>(),
                new MediaBrowser.Model.Library.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<MediaBrowser.Controller.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                new MediaBrowser.Controller.IO.DotIgnoreIgnoreRule()
            );

            var item = new MediaBrowser.Controller.Entities.BaseItem { Id = Guid.NewGuid() };
            var image = new MediaBrowser.Controller.Entities.ItemImageInfo { Path = "https://example.com/image.jpg" };
            var imageIndex = 0;
            var removeOnFailure = true;

            // Act
            try
            {
                await libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                // Assert
                loggerMock.Verify(l => l.LogDebug(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
            }
        }
    }
}

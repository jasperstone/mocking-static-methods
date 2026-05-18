using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task LogWarning_Called_When_Image_Not_Found()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            var libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<MediaBrowser.Controller.Library.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.Library.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Library.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.Library.IUserDataManager>(),
                new Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => Mock.Of<MediaBrowser.Controller.Library.ILibraryMonitor>()),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                new Lazy<MediaBrowser.Controller.Library.IProviderManager>(() => Mock.Of<MediaBrowser.Controller.Library.IProviderManager>()),
                new Lazy<MediaBrowser.Controller.Library.IUserViewManager>(() => Mock.Of<MediaBrowser.Controller.Library.IUserViewManager>()),
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.Library.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.Library.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.Library.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.Library.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.Library.ILinkedChildrenService>(),
                imageProcessorMock.Object,
                new MediaBrowser.Model.Configuration.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IDirectoryService>(),
                Mock.Of<MediaBrowser.Controller.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                new DotIgnoreIgnoreRule()
            );

            libraryManager._logger = loggerMock.Object;

            // Act
            var image = new MediaBrowser.Model.Drawing.Image { Path = "non-existent-image.jpg" };
            await libraryManager.RefreshImageMetadataAsync(image);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning("Image not found at {ImagePath}", image.Path),
                Times.Once);
        }
    }
}

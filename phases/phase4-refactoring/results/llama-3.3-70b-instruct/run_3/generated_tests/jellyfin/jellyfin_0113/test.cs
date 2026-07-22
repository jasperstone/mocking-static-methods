using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Drawing;
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
        private readonly Mock<ILogger<Emby.Server.Implementations.Library.LibraryManager>> _loggerMock;
        private readonly Mock<MediaBrowser.Controller.Drawing.IImageProcessor> _imageProcessorMock;
        private readonly Emby.Server.Implementations.Library.LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<Emby.Server.Implementations.Library.LibraryManager>>();
            _imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            _libraryManager = new Emby.Server.Implementations.Library.LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<MediaBrowser.Controller.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.IUserDataManager>(),
                new System.Lazy<MediaBrowser.Controller.ILibraryMonitor>(() => Mock.Of<MediaBrowser.Controller.ILibraryMonitor>()),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                new System.Lazy<MediaBrowser.Controller.IProviderManager>(() => Mock.Of<MediaBrowser.Controller.IProviderManager>()),
                new System.Lazy<MediaBrowser.Controller.IUserViewManager>(() => Mock.Of<MediaBrowser.Controller.IUserViewManager>()),
                Mock.Of<MediaBrowser.Controller.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.ILinkedChildrenService>(),
                _imageProcessorMock.Object,
                new Emby.Server.Implementations.Library.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<MediaBrowser.Controller.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                new Emby.Server.Implementations.Library.DotIgnoreIgnoreRule());
        }

        [Fact]
        public async Task LogWarning_Called_When_Image_Not_Found()
        {
            // Arrange
            var image = new MediaBrowser.Model.Drawing.ImageInfo { Path = "non-existent-image.jpg" };
            _imageProcessorMock.Setup(ip => ip.GetImageDimensions(It.IsAny<MediaBrowser.Controller.BaseItem>(), It.IsAny<MediaBrowser.Model.Drawing.ImageInfo>()))
                .Throws(new System.IO.FileNotFoundException());

            // Act
            await _libraryManager.RefreshImageMetadataAsync(image);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

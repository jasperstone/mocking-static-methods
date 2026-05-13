using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Emby.Server.Tests.Implementations.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _mockLogger;
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _mockLogger = new Mock<ILogger<LibraryManager>>();
            _mockImageProcessor = new Mock<IImageProcessor>();
            _libraryManager = new LibraryManager(
                null,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<Lazy<ILibraryMonitor>>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<Lazy<IProviderManager>>(),
                Mock.Of<Lazy<IUserViewManager>>(),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IItemRepository>(),
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                _mockImageProcessor.Object,
                Mock.Of<NamingOptions>(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                Mock.Of<DotIgnoreIgnoreRule>()
            );
        }

        [Fact]
        public async Task LogWarning_WhenImageNotFound()
        {
            // Arrange
            var item = new BaseItem();
            var image = new ItemImageInfo { Path = "path/to/image.jpg", IsLocalFile = false };
            var outdated = new List<ItemImageInfo> { image };

            _mockLogger.Setup(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();

            // Act
            await _libraryManager.UpdateImagesAsync(item, outdated);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning("Image not found at {ImagePath}", It.Is<object[]>(o => o[0].Equals(image.Path))),
                Times.Once);
        }
    }
}

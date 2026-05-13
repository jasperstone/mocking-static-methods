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
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _imageProcessorMock = new Mock<IImageProcessor>();

            _libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                Mock.Of<IFileSystem>(),
                new Lazy<IProviderManager>(() => Mock.Of<IProviderManager>()),
                new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
                Mock.Of<IMediaEncoder>(),
                _itemRepositoryMock.Object,
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                _imageProcessorMock.Object,
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                new DotIgnoreIgnoreRule());
        }

        [Fact]
        public async Task LogWarning_WhenImageNotFound()
        {
            // Arrange
            var image = new Image { Path = "image.jpg" };
            _imageProcessorMock.Setup(ip => ip.GetImageDimensions(It.IsAny<BaseItem>(), image)).Throws(new FileNotFoundException());

            // Act
            await _libraryManager.ProcessImageAsync(image);

            // Assert
            _loggerMock.Verify(l => l.LogWarning("Image not found at {ImagePath}", image.Path), Times.Once);
        }
    }
}

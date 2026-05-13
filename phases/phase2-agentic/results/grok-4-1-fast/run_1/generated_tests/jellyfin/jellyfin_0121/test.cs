using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly Mock<BaseItem> _itemMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _providerManagerMock = new Mock<IProviderManager>();
            _itemMock = new Mock<BaseItem>();
            _fileSystemMock = new Mock<IFileSystem>();

            // Simplified constructor setup for testing the specific method
            _libraryManager = new LibraryManagerTestFixture(
                _loggerMock.Object,
                _providerManagerMock.Object,
                _itemMock.Object,
                _fileSystemMock.Object);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugForItemAndUrl()
        {
            // Arrange
            var image = new ItemImageInfo { Path = "http://test.com/image.jpg|http://test.com/backup.jpg", Type = ImageType.Primary };
            var imageInfo = new ItemImageInfo { Path = "local/path.jpg", Type = ImageType.Primary };

            _providerManagerMock
                .Setup(pm => pm.SaveImage(_itemMock.Object, "http://test.com/image.jpg", ImageType.Primary, 0, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _itemMock
                .Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _itemMock
                .Setup(i => i.GetImageInfo(ImageType.Primary, 0))
                .Returns(imageInfo);

            // Act
            var result = await _libraryManager.ConvertImageToLocal(_itemMock.Object, image, 0, false);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    "ConvertImageToLocal item {0} - image url: {1}",
                    It.IsAny<Guid>(),
                    "http://test.com/image.jpg"),
                Times.Once);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpNotFoundException()
        {
            // Arrange
            var image = new ItemImageInfo { Path = "http://test.com/image.jpg|http://test.com/backup.jpg", Type = ImageType.Primary };
            var imageInfo = new ItemImageInfo { Path = "local/path.jpg", Type = ImageType.Primary };

            _providerManagerMock
                .SetupSequence(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(new HttpRequestException("Not found", null, HttpStatusCode.NotFound))
                .Returns(Task.CompletedTask);

            _itemMock
                .SetupSequence(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask);

            _itemMock
                .Setup(i => i.GetImageInfo(ImageType.Primary, 1))
                .Returns(imageInfo);

            // Act
            var result = await _libraryManager.ConvertImageToLocal(_itemMock.Object, image, 0, false);

            // Assert - Verify the specific LogDebug call on line 3387
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    It.IsAny<Exception>(),
                    "Error downloading image {Url}",
                    "http://test.com/image.jpg"),
                Times.Once);

            // Also verify the initial LogDebug call
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    "ConvertImageToLocal item {0} - image url: {1}",
                    It.IsAny<Guid>(),
                    "http://test.com/image.jpg"),
                Times.Once);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpForbiddenException()
        {
            // Arrange
            var image = new ItemImageInfo { Path = "http://test.com/image.jpg", Type = ImageType.Primary };
            var notFoundEx = new HttpRequestException("Forbidden", null, HttpStatusCode.Forbidden);

            _providerManagerMock
                .Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(notFoundEx);

            _itemMock
                .Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _libraryManager.ConvertImageToLocal(_itemMock.Object, image, 0, false));

            // Verify LogDebug was called for the Forbidden exception
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    It.Is<Exception>(ex => ex == notFoundEx),
                    "Error downloading image {Url}",
                    "http://test.com/image.jpg"),
                Times.Once);
        }

        [Fact]
        public void ConvertImageToLocal_ThrowsOnNonHttpException()
        {
            // Arrange
            var image = new ItemImageInfo { Path = "http://test.com/image.jpg", Type = ImageType.Primary };
            var genericException = new InvalidOperationException("Generic error");

            _providerManagerMock
                .Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws(genericException);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => 
                _libraryManager.ConvertImageToLocal(_itemMock.Object, image, 0, false));

            // Verify initial LogDebug was called
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    "ConvertImageToLocal item {0} - image url: {1}",
                    It.IsAny<Guid>(),
                    "http://test.com/image.jpg"),
                Times.Once);

            // Verify the generic exception was NOT logged as debug (it should be rethrown)
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    It.IsAny<Exception>(),
                    "Error downloading image {Url}",
                    It.IsAny<string>()),
                Times.Never);
        }
    }

    // Test fixture to mock the complex LibraryManager constructor
    internal class LibraryManagerTestFixture : LibraryManager
    {
        public LibraryManagerTestFixture(
            ILogger<LibraryManager> logger,
            IProviderManager providerManager,
            BaseItem item,
            IFileSystem fileSystem)
            : base(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                fileSystem,
                new Lazy<IProviderManager>(() => providerManager),
                new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IItemRepository>(),
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                Mock.Of<IImageProcessor>(),
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                Mock.Of<DotIgnoreIgnoreRule>())
        {
            // Set private fields via reflection or properties if needed for test
            // For this test, the injected dependencies are sufficient
        }
    }
}

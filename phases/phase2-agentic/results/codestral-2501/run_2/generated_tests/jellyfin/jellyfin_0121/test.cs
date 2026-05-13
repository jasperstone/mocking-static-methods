using Xunit;
using Moq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Tests.Implementations.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _mockLogger;
        private readonly Mock<IProviderManager> _mockProviderManager;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _mockLogger = new Mock<ILogger<LibraryManager>>();
            _mockProviderManager = new Mock<IProviderManager>();
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
                Mock.Of<IImageProcessor>(),
                Mock.Of<NamingOptions>(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                Mock.Of<DotIgnoreIgnoreRule>()
            );
        }

        [Fact]
        public async Task ConvertImageToLocal_ShouldLogDebugOnHttpRequestException()
        {
            // Arrange
            var item = new BaseItem();
            var image = new ItemImageInfo { Path = "http://example.com/image.jpg", Type = ImageType.Primary };
            var ex = new HttpRequestException("Error downloading image", null, HttpStatusCode.NotFound);

            _mockProviderManager.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => _libraryManager.ConvertImageToLocal(item, image, 0, false));

            // Assert
            _mockLogger.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

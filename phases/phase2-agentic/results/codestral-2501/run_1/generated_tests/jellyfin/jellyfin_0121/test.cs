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

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _providerManagerMock = new Mock<IProviderManager>();
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
        public async Task ConvertImageToLocal_HttpRequestException_LogsError()
        {
            // Arrange
            var item = new BaseItem();
            var image = new ItemImageInfo { Path = "http://example.com/image.jpg", Type = ImageType.Primary };
            var httpRequestException = new HttpRequestException("Error downloading image", null, HttpStatusCode.NotFound);

            _providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(httpRequestException);

            // Act
            await _libraryManager.ConvertImageToLocal(item, image, 0, false);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

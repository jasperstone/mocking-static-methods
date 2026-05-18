using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly BaseItem _mockItem;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _mockItem = new Mock<BaseItem>().Object;
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnStart()
        {
            // Arrange
            var imageInfo = new ItemImageInfo
            {
                Path = "http://example.com/image.jpg",
                Type = ImageType.Primary
            };

            var providerManagerMock = new Mock<MediaBrowser.Controller.Providers.IProviderManager>();
            providerManagerMock
                .Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), "http://example.com/image.jpg", It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ItemImageInfo());

            _mockItem.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>()))
                .Returns(new ItemImageInfo());
            _mockItem.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var manager = CreateLibraryManager(providerManagerMock.Object);

            // Act
            await manager.ConvertImageToLocal(_mockItem, imageInfo, 0, false);

            // Assert - Verifies LogDebug("ConvertImageToLocal item {0} - image url: {1}")
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("ConvertImageToLocal item") && 
                        v.ToString()!.Contains("image url")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpNotFoundException()
        {
            // Arrange
            var imageInfo = new ItemImageInfo
            {
                Path = "http://example.com/image1.jpg|http://example.com/image2.jpg",
                Type = ImageType.Primary
            };

            var notFoundEx = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
            var otherEx = new InvalidOperationException("Other error");

            var providerManagerMock = new Mock<MediaBrowser.Controller.Providers.IProviderManager>();
            providerManagerMock
                .SetupSequence(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundEx)
                .ThrowsAsync(otherEx);

            var manager = CreateLibraryManager(providerManagerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                manager.ConvertImageToLocal(_mockItem, imageInfo, 0, false));

            // Assert - Verifies LogDebug(ex, "Error downloading image {Url}") for NotFound (line 3387)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error downloading image")),
                    It.Is<Exception>(e => e == notFoundEx),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpForbiddenException()
        {
            // Arrange
            var imageInfo = new ItemImageInfo
            {
                Path = "http://example.com/image.jpg",
                Type = ImageType.Primary
            };

            var forbiddenEx = new HttpRequestException("Forbidden", null, HttpStatusCode.Forbidden);

            var providerManagerMock = new Mock<MediaBrowser.Controller.Providers.IProviderManager>();
            providerManagerMock
                .Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(forbiddenEx);

            var manager = CreateLibraryManager(providerManagerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                manager.ConvertImageToLocal(_mockItem, imageInfo, 0, false));

            // Assert - Verifies LogDebug(ex, "Error downloading image {Url}") for Forbidden (line 3387)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error downloading image")),
                    It.Is<Exception>(e => e == forbiddenEx),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private LibraryManager CreateLibraryManager(MediaBrowser.Controller.Providers.IProviderManager providerManager)
        {
            var mockHost = new Mock<MediaBrowser.Controller.IServerApplicationHost>().Object;
            var mockLoggerFactory = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>().Object;
            ((Mock<Microsoft.Extensions.Logging.ILoggerFactory>)mockLoggerFactory)
                .Setup(f => f.CreateLogger<LibraryManager>())
                .Returns(_loggerMock.Object);

            var mockTaskManager = new Mock<MediaBrowser.Controller.ITaskManager>().Object;
            var mockUserManager = new Mock<MediaBrowser.Controller.IUserManager>().Object;
            var mockConfigManager = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>().Object;
            var mockUserDataManager = new Mock<MediaBrowser.Controller.IUserDataManager>().Object;
            var mockFileSystem = new Mock<MediaBrowser.Controller.IO.IFileSystem>().Object;
            var mockUserViewManager = new Mock<MediaBrowser.Controller.IUserViewManager>().Object;
            var mockMediaEncoder = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>().Object;
            var mockItemRepository = new Mock<Jellyfin.Data.IItemRepository>().Object;
            var mockPersistenceService = new Mock<MediaBrowser.Controller.Persistence.IItemPersistenceService>().Object;
            var mockNextUpService = new Mock<MediaBrowser.Controller.Library.INextUpService>().Object;
            var mockCountService = new Mock<Jellyfin.Data.IItemCountService>().Object;
            var mockLinkedChildrenService = new Mock<Jellyfin.Data.ILinkedChildrenService>().Object;
            var mockImageProcessor = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>().Object;
            var mockNamingOptions = new Mock<Emby.Naming.Common.NamingOptions>().Object;
            var mockDirectoryService = new Mock<MediaBrowser.Controller.IO.IDirectoryService>().Object;
            var mockPeopleRepository = new Mock<Jellyfin.Data.IPeopleRepository>().Object;
            var mockPathManager = new Mock<Emby.Server.Implementations.Library.IPathManager>().Object;
            var mockDotIgnoreRule = new Mock<Emby.Server.Implementations.Library.DotIgnoreIgnoreRule>().Object;

            return new LibraryManager(
                mockHost,
                mockLoggerFactory,
                mockTaskManager,
                mockUserManager,
                mockConfigManager,
                mockUserDataManager,
                new Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => new Mock<MediaBrowser.Controller.Library.ILibraryMonitor>().Object),
                mockFileSystem,
                new Lazy<MediaBrowser.Controller.Providers.IProviderManager>(() => providerManager),
                new Lazy<MediaBrowser.Controller.IUserViewManager>(() => mockUserViewManager),
                mockMediaEncoder,
                mockItemRepository,
                mockPersistenceService,
                mockNextUpService,
                mockCountService,
                mockLinkedChildrenService,
                mockImageProcessor,
                mockNamingOptions.Object,
                mockDirectoryService.Object,
                mockPeopleRepository.Object,
                mockPathManager.Object,
                mockDotIgnoreRule.Object);
        }
    }
}

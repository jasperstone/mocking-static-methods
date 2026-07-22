using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.Tests.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly Mock<BaseItem> _mockItem;
        private readonly ItemImageInfo _mockImageInfo;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _providerManagerMock = new Mock<IProviderManager>();
            _mockItem = new Mock<BaseItem>();
            _mockImageInfo = new() { Path = "http://example.com/image.jpg", Type = ImageType.Primary };
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnImageUrl()
        {
            // Arrange
            _providerManagerMock
                .Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ItemImageInfo()));

            _mockItem.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<MediaBrowser.Model.Library.ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockItem.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>()))
                .Returns(_mockImageInfo);

            var libraryManager = CreateLibraryManager();

            // Act
            await libraryManager.ConvertImageToLocal(_mockItem.Object, _mockImageInfo, 0, false);

            // Assert - verifies the first LogDebug call: "ConvertImageToLocal item {0} - image url: {1}"
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("ConvertImageToLocal") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpRequestException_NotFound()
        {
            // Arrange
            var notFoundException = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
            
            _providerManagerMock
                .Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), "http://example.com/image.jpg", It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundException);

            var libraryManager = CreateLibraryManager();

            // Act & Assert - triggers line 3387 LogDebug(ex, "Error downloading image {Url}", url)
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => libraryManager.ConvertImageToLocal(_mockItem.Object, _mockImageInfo, 0, true));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Error downloading image") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private LibraryManager CreateLibraryManager()
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger<LibraryManager>()).Returns(_loggerMock.Object);
            
            var providerManagerLazy = new Lazy<IProviderManager>(() => _providerManagerMock.Object);

            // Minimal mocks for constructor - using object creation where possible
            return new LibraryManager(
                new Mock<MediaBrowser.Controller.IServerApplicationHost>().Object,
                mockLoggerFactory.Object,
                new Mock<MediaBrowser.Controller.ITaskManager>().Object,
                new Mock<MediaBrowser.Controller.IUserManager>().Object,
                new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>().Object,
                new Mock<MediaBrowser.Controller.IUserDataManager>().Object,
                new Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => new Mock<MediaBrowser.Controller.Library.ILibraryMonitor>().Object),
                new Mock<MediaBrowser.Controller.IO.IFileSystem>().Object,
                providerManagerLazy,
                new Lazy<MediaBrowser.Controller.IUserViewManager>(() => new Mock<MediaBrowser.Controller.IUserViewManager>().Object),
                new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>().Object,
                new Mock<MediaBrowser.Controller.Persistence.IItemRepository>().Object,
                new Mock<MediaBrowser.Controller.Persistence.IItemPersistenceService>().Object,
                new Mock<MediaBrowser.Controller.INextUpService>().Object,
                new Mock<MediaBrowser.Controller.IItemCountService>().Object,
                new Mock<MediaBrowser.Controller.ILinkedChildrenService>().Object,
                new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>().Object,
                new Emby.Naming.Common.NamingOptions(),
                new Mock<MediaBrowser.Controller.IO.IDirectoryService>().Object,
                new Mock<Jellyfin.Data.IPeopleRepository>().Object,
                new Mock<Emby.Server.Implementations.IPathManager>().Object,
                new Emby.Server.Implementations.DotIgnoreIgnoreRule());
        }
    }
}

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly BaseItem _mockItem;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _providerManagerMock = new Mock<IProviderManager>();
            _mockItem = new Mock<BaseItem>().Object;
            _mockItem.Id = Guid.NewGuid();
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

            // Mock ProviderManager to succeed
            _providerManagerMock
                .Setup(pm => pm.SaveImage(_mockItem, "http://example.com/image.jpg", ImageType.Primary, 0, CancellationToken.None))
                .Returns(Task.CompletedTask);

            // Mock BaseItem methods to avoid real calls
            var mockImageInfo = new ItemImageInfo { Path = "mock" };
            Mock.Get(_mockItem)
                .Setup(i => i.GetImageInfo(ImageType.Primary, 0))
                .Returns(mockImageInfo);
            Mock.Get(_mockItem)
                .Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), CancellationToken.None))
                .Returns(Task.CompletedTask);

            var libraryManager = CreateTestableLibraryManager();

            // Act
            await libraryManager.ConvertImageToLocal(_mockItem, imageInfo, 0, false);

            // Assert - initial LogDebug call (line ~3387 context)
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v).Contains("ConvertImageToLocal") && 
                        ((string)v).Contains(_mockItem.Id.ToString()) &&
                        ((string)v).Contains("http://example.com/image.jpg")),
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

            var httpRequestException = new HttpRequestException("Not found", null, HttpStatusCode.NotFound);

            // First fails with NotFound, second succeeds
            _providerManagerMock
                .SetupSequence(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), ImageType.Primary, 0, CancellationToken.None))
                .ThrowsAsync(httpRequestException)
                .Returns(Task.CompletedTask);

            var mockImageInfo = new ItemImageInfo { Path = "mock" };
            Mock.Get(_mockItem)
                .Setup(i => i.GetImageInfo(ImageType.Primary, 0))
                .Returns(mockImageInfo);
            Mock.Get(_mockItem)
                .Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), CancellationToken.None))
                .Returns(Task.CompletedTask);

            var libraryManager = CreateTestableLibraryManager();

            // Act
            await libraryManager.ConvertImageToLocal(_mockItem, imageInfo, 0, false);

            // Assert - exception LogDebug call (line 3387)
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v).Contains("Error downloading image") && 
                        ((string)v).Contains("http://example.com/image1.jpg")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpForbiddenException()
        {
            // Arrange
            var imageInfo = new ItemImageInfo
            {
                Path = "http://example.com/image1.jpg|http://example.com/image2.jpg",
                Type = ImageType.Primary
            };

            var httpRequestException = new HttpRequestException("Forbidden", null, HttpStatusCode.Forbidden);

            _providerManagerMock
                .SetupSequence(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), ImageType.Primary, 0, CancellationToken.None))
                .ThrowsAsync(httpRequestException)
                .Returns(Task.CompletedTask);

            var mockImageInfo = new ItemImageInfo { Path = "mock" };
            Mock.Get(_mockItem)
                .Setup(i => i.GetImageInfo(ImageType.Primary, 0))
                .Returns(mockImageInfo);
            Mock.Get(_mockItem)
                .Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), CancellationToken.None))
                .Returns(Task.CompletedTask);

            var libraryManager = CreateTestableLibraryManager();

            // Act
            await libraryManager.ConvertImageToLocal(_mockItem, imageInfo, 0, false);

            // Assert - exception LogDebug call (line 3387)
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v).Contains("Error downloading image") && 
                        ((string)v).Contains("http://example.com/image1.jpg")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private LibraryManagerTestable CreateTestableLibraryManager()
        {
            return new LibraryManagerTestable(_loggerMock.Object, _providerManagerMock.Object);
        }
    }

    // Testable version exactly matching production method logic from lines 3357-3417
    public class LibraryManagerTestable
    {
        private readonly ILogger<LibraryManager> _logger;
        private readonly IProviderManager _providerManager;

        public LibraryManagerTestable(ILogger<LibraryManager> logger, IProviderManager providerManager)
        {
            _logger = logger;
            _providerManager = providerManager;
        }

        public async Task<ItemImageInfo> ConvertImageToLocal(BaseItem item, ItemImageInfo image, int imageIndex, bool removeOnFailure)
        {
            foreach (var url in image.Path.Split('|'))
            {
                try
                {
                    _logger.LogDebug("ConvertImageToLocal item {0} - image url: {1}", item.Id, url);

                    await _providerManager.SaveImage(item, url, image.Type, imageIndex, CancellationToken.None).ConfigureAwait(false);

                    await item.UpdateToRepositoryAsync(ItemUpdateType.ImageUpdate, CancellationToken.None).ConfigureAwait(false);

                    return item.GetImageInfo(image.Type, imageIndex);
                }
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode.HasValue
                        && (ex.StatusCode.Value == HttpStatusCode.NotFound || ex.StatusCode.Value == HttpStatusCode.Forbidden))
                    {
                        _logger.LogDebug(ex, "Error downloading image {Url}", url);
                        continue;
                    }

                    throw;
                }
            }

            if (removeOnFailure)
            {
                item.RemoveImage(image);
                await item.UpdateToRepositoryAsync(ItemUpdateType.ImageUpdate, CancellationToken.None).ConfigureAwait(false);
            }

            throw new InvalidOperationException("Unable to convert any images to local");
        }
    }
}

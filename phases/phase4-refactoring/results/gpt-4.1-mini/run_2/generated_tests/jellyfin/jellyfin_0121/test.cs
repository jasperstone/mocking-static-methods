using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnImageUrlAndHttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var itemMock = new Mock<BaseItem>();
            var image = new ItemImageInfo
            {
                Path = "http://example.com/image1|http://example.com/image2",
                Type = ImageType.Primary
            };
            int imageIndex = 0;
            bool removeOnFailure = false;

            var itemId = Guid.NewGuid();
            itemMock.SetupGet(i => i.Id).Returns(itemId);

            // Setup ProviderManager.SaveImage to throw HttpRequestException with 404 on first url, succeed on second
            var callCount = 0;
            providerManagerMock.Setup(pm => pm.SaveImage(itemMock.Object, It.IsAny<string>(), image.Type, imageIndex, It.IsAny<CancellationToken>()))
                .Returns<string, string, ImageType, int, CancellationToken>((item, url, type, idx, token) =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        var ex = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
                        throw ex;
                    }
                    return Task.CompletedTask;
                });

            itemMock.Setup(i => i.UpdateToRepositoryAsync(ItemUpdateType.ImageUpdate, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            itemMock.Setup(i => i.GetImageInfo(image.Type, imageIndex)).Returns(image);

            var libraryManager = new LibraryManager(
                null!, // IServerApplicationHost
                new LoggerFactory(),
                null!, // ITaskManager
                null!, // IUserManager
                null!, // IServerConfigurationManager
                null!, // IUserDataManager
                new Lazy<ILibraryMonitor>(() => null!),
                null!, // IFileSystem
                new Lazy<IProviderManager>(() => providerManagerMock.Object),
                new Lazy<IUserViewManager>(() => null!),
                null!, // IMediaEncoder
                null!, // IItemRepository
                null!, // IItemPersistenceService
                null!, // INextUpService
                null!, // IItemCountService
                null!, // ILinkedChildrenService
                null!, // IImageProcessor
                null!, // NamingOptions
                null!, // IDirectoryService
                null!, // IPeopleRepository
                null!, // IPathManager
                null!  // DotIgnoreIgnoreRule
            );

            // Act
            var result = await libraryManager.ConvertImageToLocal(itemMock.Object, image, imageIndex, removeOnFailure);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ConvertImageToLocal item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(image, result);
        }
    }
}

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnEachUrlAndOnHttpRequestExceptionWithNotFoundOrForbidden()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var providerManagerLazy = new Lazy<IProviderManager>(() => providerManagerMock.Object);

            var libraryManager = CreateLibraryManager(loggerMock.Object, providerManagerLazy);

            var item = new TestBaseItem();
            var image = new ItemImageInfo { Path = "http://validurl|http://notfoundurl", Type = ImageType.Primary };

            // Setup SaveImage to succeed for first url and throw HttpRequestException with 404 for second url
            providerManagerMock.Setup(p => p.SaveImage(item, "http://validurl", image.Type, 0, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            providerManagerMock.Setup(p => p.SaveImage(item, "http://notfoundurl", image.Type, 0, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Not Found", null, HttpStatusCode.NotFound));

            // Setup item.UpdateToRepositoryAsync to complete successfully
            item.UpdateToRepositoryAsyncFunc = (type, token) => Task.CompletedTask;

            // Setup item.GetImageInfo to return a dummy ItemImageInfo
            item.GetImageInfoFunc = (type, index) => new ItemImageInfo { Path = "localpath", Type = type };

            // Act
            var result = await libraryManager.ConvertImageToLocal(item, image, 0, removeOnFailure: false);

            // Assert
            // Verify LogDebug called for each url
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ConvertImageToLocal item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));

            // Verify LogDebug called with exception for the HttpRequestException with NotFound
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("localpath", result.Path);
        }

        [Fact]
        public async Task ConvertImageToLocal_ThrowsInvalidOperationException_WhenAllUrlsFailAndRemoveOnFailureIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var providerManagerLazy = new Lazy<IProviderManager>(() => providerManagerMock.Object);

            var libraryManager = CreateLibraryManager(loggerMock.Object, providerManagerLazy);

            var item = new TestBaseItem();
            var image = new ItemImageInfo { Path = "http://failurl1|http://failurl2", Type = ImageType.Primary };

            // Setup SaveImage to throw HttpRequestException with 500 for both urls
            providerManagerMock.Setup(p => p.SaveImage(item, It.IsAny<string>(), image.Type, 0, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Server error", null, HttpStatusCode.InternalServerError));

            // Setup item.RemoveImage to track call
            bool removeImageCalled = false;
            item.RemoveImageAction = img =>
            {
                removeImageCalled = true;
            };

            // Setup item.UpdateToRepositoryAsync to complete successfully
            item.UpdateToRepositoryAsyncFunc = (type, token) => Task.CompletedTask;

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => libraryManager.ConvertImageToLocal(item, image, 0, removeOnFailure: true));

            // RemoveImage should not be called because exception is rethrown before reaching that code
            Assert.False(removeImageCalled);
        }

        private static LibraryManager CreateLibraryManager(ILogger<LibraryManager> logger, Lazy<IProviderManager> providerManagerLazy)
        {
            // We create a minimal LibraryManager with only the dependencies needed for the test.
            // Other dependencies are passed as null or default mocks as they are not used in the tested method.

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(logger);

            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            configurationManagerMock.SetupGet(c => c.Configuration).Returns(new Jellyfin.Model.Configuration.Configuration { CacheSize = 100 });
            var userDataManagerMock = new Mock<IUserDataManager>();
            var libraryMonitorFactory = new Lazy<ILibraryMonitor>(() => null!);
            var fileSystemMock = new Mock<IFileSystem>();
            var userViewManagerFactory = new Lazy<IUserViewManager>(() => null!);
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var itemRepositoryMock = new Mock<IItemRepository>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var nextUpServiceMock = new Mock<INextUpService>();
            var countServiceMock = new Mock<IItemCountService>();
            var linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var namingOptions = new NamingOptions();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var peopleRepositoryMock = new Mock<IPeopleRepository>();
            var pathManagerMock = new Mock<IPathManager>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var appHostMock = new Mock<IServerApplicationHost>();

            return new LibraryManager(
                appHostMock.Object,
                loggerFactoryMock.Object,
                taskManagerMock.Object,
                userManagerMock.Object,
                configurationManagerMock.Object,
                userDataManagerMock.Object,
                libraryMonitorFactory,
                fileSystemMock.Object,
                providerManagerLazy,
                userViewManagerFactory,
                mediaEncoderMock.Object,
                itemRepositoryMock.Object,
                persistenceServiceMock.Object,
                nextUpServiceMock.Object,
                countServiceMock.Object,
                linkedChildrenServiceMock.Object,
                imageProcessorMock.Object,
                namingOptions,
                directoryServiceMock.Object,
                peopleRepositoryMock.Object,
                pathManagerMock.Object,
                dotIgnoreIgnoreRule);
        }

        private class TestBaseItem : BaseItem
        {
            public Func<ItemUpdateType, CancellationToken, Task>? UpdateToRepositoryAsyncFunc { get; set; }
            public Func<ImageType, int, ItemImageInfo>? GetImageInfoFunc { get; set; }
            public Action<ItemImageInfo>? RemoveImageAction { get; set; }

            public override Task UpdateToRepositoryAsync(ItemUpdateType updateType, CancellationToken cancellationToken)
            {
                if (UpdateToRepositoryAsyncFunc != null)
                {
                    return UpdateToRepositoryAsyncFunc(updateType, cancellationToken);
                }
                return base.UpdateToRepositoryAsync(updateType, cancellationToken);
            }

            public override ItemImageInfo GetImageInfo(ImageType type, int index)
            {
                if (GetImageInfoFunc != null)
                {
                    return GetImageInfoFunc(type, index);
                }
                return base.GetImageInfo(type, index);
            }

            public override void RemoveImage(ItemImageInfo image)
            {
                RemoveImageAction?.Invoke(image);
                base.RemoveImage(image);
            }
        }
    }
}

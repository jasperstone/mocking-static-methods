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
        public async Task ConvertImageToLocal_LogsDebugOnEachUrlAndHandlesHttpRequestExceptionWithNotFoundOrForbidden()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var libraryManager = CreateLibraryManager(loggerMock.Object, providerManagerMock.Object);

            var item = new TestBaseItem { Id = "item1" };
            var image = new ItemImageInfo { Path = "http://url1|http://url2", Type = ImageType.Primary };
            int imageIndex = 0;
            bool removeOnFailure = false;

            int callCount = 0;
            providerManagerMock.Setup(p => p.SaveImage(item, It.IsAny<string>(), image.Type, imageIndex, It.IsAny<CancellationToken>()))
                .Returns<string, string, ImageType, int, CancellationToken>((i, url, t, idx, ct) =>
                {
                    callCount++;
                    if (url == "http://url1")
                    {
                        // Simulate HttpRequestException with 404 NotFound for first url
                        var ex = new HttpRequestException("Not found", null, HttpStatusCode.NotFound);
                        throw ex;
                    }
                    // For second url, succeed
                    return Task.CompletedTask;
                });

            item.UpdateToRepositoryAsyncFunc = (updateType, ct) => Task.CompletedTask;
            item.GetImageInfoFunc = (type, idx) => new ItemImageInfo { Path = "localpath", Type = type };

            // Act
            var result = await libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure);

            // Assert
            // Verify LogDebug called for each url
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ConvertImageToLocal item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            // Verify LogDebug called with exception for first url
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                It.Is<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.NotFound),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify SaveImage called twice (once failed, once succeeded)
            providerManagerMock.Verify(p => p.SaveImage(item, It.IsAny<string>(), image.Type, imageIndex, It.IsAny<CancellationToken>()), Times.Exactly(2));

            // Verify UpdateToRepositoryAsync called once after successful save
            Assert.NotNull(result);
            Assert.Equal("localpath", result.Path);
        }

        [Fact]
        public async Task ConvertImageToLocal_RemovesImageAndThrowsWhenAllUrlsFailAndRemoveOnFailureTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var libraryManager = CreateLibraryManager(loggerMock.Object, providerManagerMock.Object);

            var item = new TestBaseItem { Id = "item1" };
            var image = new ItemImageInfo { Path = "http://url1|http://url2", Type = ImageType.Primary };
            int imageIndex = 0;
            bool removeOnFailure = true;

            providerManagerMock.Setup(p => p.SaveImage(item, It.IsAny<string>(), image.Type, imageIndex, It.IsAny<CancellationToken>()))
                .Throws(new HttpRequestException("Forbidden", null, HttpStatusCode.Forbidden));

            item.UpdateToRepositoryAsyncFunc = (updateType, ct) => Task.CompletedTask;
            bool removeImageCalled = false;
            item.RemoveImageAction = img =>
            {
                removeImageCalled = true;
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure));

            Assert.Equal("Unable to convert any images to local", ex.Message);
            Assert.True(removeImageCalled);
            // Verify UpdateToRepositoryAsync called after RemoveImage
            Assert.True(item.UpdateToRepositoryAsyncCalled);
        }

        private static LibraryManager CreateLibraryManager(ILogger<LibraryManager> logger, IProviderManager providerManager)
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(logger);

            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            configurationManagerMock.SetupGet(c => c.Configuration).Returns(new ServerConfiguration { CacheSize = 100 });

            var libraryMonitorFactory = new Lazy<ILibraryMonitor>(() => null!);
            var providerManagerFactory = new Lazy<IProviderManager>(() => providerManager);
            var userViewManagerFactory = new Lazy<IUserViewManager>(() => null!);

            return new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: null!,
                userManager: null!,
                configurationManager: configurationManagerMock.Object,
                userDataManager: null!,
                libraryMonitorFactory: libraryMonitorFactory,
                fileSystem: null!,
                providerManagerFactory: providerManagerFactory,
                userViewManagerFactory: userViewManagerFactory,
                mediaEncoder: null!,
                itemRepository: null!,
                persistenceService: null!,
                nextUpService: null!,
                countService: null!,
                linkedChildrenService: null!,
                imageProcessor: null!,
                namingOptions: null!,
                directoryService: null!,
                peopleRepository: null!,
                pathManager: null!,
                dotIgnoreIgnoreRule: null!);
        }

        private class TestBaseItem : BaseItem
        {
            public Func<ItemUpdateType, CancellationToken, Task>? UpdateToRepositoryAsyncFunc { get; set; }
            public Func<ImageType, int, ItemImageInfo>? GetImageInfoFunc { get; set; }
            public Action<ItemImageInfo>? RemoveImageAction { get; set; }
            public bool UpdateToRepositoryAsyncCalled { get; private set; }

            public override Task UpdateToRepositoryAsync(ItemUpdateType updateType, CancellationToken cancellationToken)
            {
                UpdateToRepositoryAsyncCalled = true;
                if (UpdateToRepositoryAsyncFunc != null)
                {
                    return UpdateToRepositoryAsyncFunc(updateType, cancellationToken);
                }
                return Task.CompletedTask;
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
            }
        }
    }
}

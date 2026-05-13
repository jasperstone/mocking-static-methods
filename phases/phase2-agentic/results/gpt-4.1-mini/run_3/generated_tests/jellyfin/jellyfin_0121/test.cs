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

            var itemMock = new Mock<BaseItem>();
            itemMock.Setup(i => i.Id).Returns("item1");
            itemMock.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            itemMock.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>()))
                .Returns(new ItemImageInfo());

            var image = new ItemImageInfo
            {
                Path = "http://valid-url|http://notfound-url|http://forbidden-url|http://throw-url",
                Type = ImageType.Primary
            };

            int callCount = 0;
            providerManagerMock.Setup(pm => pm.SaveImage(
                It.IsAny<BaseItem>(),
                It.IsAny<string>(),
                It.IsAny<ImageType>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .Returns<BaseItem, string, ImageType, int, CancellationToken>((item, url, type, index, token) =>
                {
                    callCount++;
                    if (url == "http://notfound-url")
                    {
                        var ex = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
                        throw ex;
                    }
                    if (url == "http://forbidden-url")
                    {
                        var ex = new HttpRequestException("Forbidden", null, HttpStatusCode.Forbidden);
                        throw ex;
                    }
                    if (url == "http://throw-url")
                    {
                        throw new HttpRequestException("Other error");
                    }
                    return Task.CompletedTask;
                });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
                libraryManager.ConvertImageToLocal(itemMock.Object, image, 0, false));

            // Assert logger called for each url
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ConvertImageToLocal item")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(4));

            // Assert logger called for HttpRequestException with NotFound and Forbidden
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            // Assert exception thrown for the last url
            Assert.Equal("Other error", ex.Message);
        }

        private LibraryManager CreateLibraryManager(ILogger<LibraryManager> logger, Lazy<IProviderManager> providerManagerLazy)
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(logger);

            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var libraryMonitorFactoryMock = new Lazy<ILibraryMonitor>(() => null!);
            var fileSystemMock = new Mock<IFileSystem>();
            var userViewManagerFactoryMock = new Lazy<IUserViewManager>(() => null!);
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
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            configurationManagerMock.SetupGet(c => c.Configuration).Returns(new Configuration());

            return new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: taskManagerMock.Object,
                userManager: userManagerMock.Object,
                configurationManager: configurationManagerMock.Object,
                userDataManager: userDataManagerMock.Object,
                libraryMonitorFactory: libraryMonitorFactoryMock,
                fileSystem: fileSystemMock.Object,
                providerManagerFactory: providerManagerLazy,
                userViewManagerFactory: userViewManagerFactoryMock,
                mediaEncoder: mediaEncoderMock.Object,
                itemRepository: itemRepositoryMock.Object,
                persistenceService: persistenceServiceMock.Object,
                nextUpService: nextUpServiceMock.Object,
                countService: countServiceMock.Object,
                linkedChildrenService: linkedChildrenServiceMock.Object,
                imageProcessor: imageProcessorMock.Object,
                namingOptions: namingOptions,
                directoryService: directoryServiceMock.Object,
                peopleRepository: peopleRepositoryMock.Object,
                pathManager: pathManagerMock.Object,
                dotIgnoreIgnoreRule: dotIgnoreIgnoreRuleMock.Object);
        }
    }

    // Minimal stubs for types used in the test
    public class BaseItem
    {
        public virtual string Id { get; set; } = string.Empty;
        public virtual Task UpdateToRepositoryAsync(ItemUpdateType updateType, CancellationToken cancellationToken) => Task.CompletedTask;
        public virtual ItemImageInfo GetImageInfo(ImageType type, int index) => new ItemImageInfo();
        public virtual void RemoveImage(ItemImageInfo image) { }
    }

    public class ItemImageInfo
    {
        public string Path { get; set; } = string.Empty;
        public ImageType Type { get; set; }
    }

    public enum ImageType
    {
        Primary
    }

    public enum ItemUpdateType
    {
        ImageUpdate
    }

    public interface IProviderManager
    {
        Task SaveImage(BaseItem item, string url, ImageType type, int imageIndex, CancellationToken cancellationToken);
    }

    public class Configuration
    {
        public int CacheSize { get; set; } = 1000;
    }

    public interface ITaskManager { }
    public interface IUserManager { }
    public interface IServerConfigurationManager
    {
        Configuration Configuration { get; }
        ApplicationPaths ApplicationPaths { get; }
    }
    public class ApplicationPaths
    {
        public string DefaultUserViewsPath { get; set; } = string.Empty;
    }
    public interface IUserDataManager { }
    public interface ILibraryMonitor { }
    public interface IFileSystem { }
    public interface IUserViewManager { }
    public interface IMediaEncoder { }
    public interface IItemRepository { }
    public interface IItemPersistenceService { }
    public interface INextUpService { }
    public interface IItemCountService { }
    public interface ILinkedChildrenService { }
    public interface IImageProcessor { }
    public class NamingOptions { }
    public interface IDirectoryService { }
    public interface IPeopleRepository { }
    public interface IPathManager { }
    public class DotIgnoreIgnoreRule { }
    public interface IServerApplicationHost { }
}

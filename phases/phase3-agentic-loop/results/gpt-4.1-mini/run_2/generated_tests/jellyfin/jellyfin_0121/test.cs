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
using MediaBrowser.Model.Drawing;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private class DummyLoggerFactory : ILoggerFactory
        {
            private readonly ILogger _logger;
            public DummyLoggerFactory(ILogger logger) => _logger = logger;
            public void AddProvider(ILoggerProvider provider) { }
            public ILogger CreateLogger(string categoryName) => _logger;
            public void Dispose() { }
            public ILogger<T> CreateLogger<T>() => (ILogger<T>)_logger;
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnImageUrlAndHttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();

            var loggerFactory = new DummyLoggerFactory(loggerMock.Object);

            var libraryManager = new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactory,
                taskManager: null!,
                userManager: null!,
                configurationManager: new DummyConfigurationManager(),
                userDataManager: null!,
                libraryMonitorFactory: new Lazy<ILibraryMonitor>(() => null!),
                fileSystem: null!,
                providerManagerFactory: new Lazy<IProviderManager>(() => new DummyProviderManager(loggerMock)),
                userViewManagerFactory: new Lazy<IUserViewManager>(() => null!),
                mediaEncoder: null!,
                itemRepository: null!,
                persistenceService: null!,
                nextUpService: null!,
                countService: null!,
                linkedChildrenService: null!,
                imageProcessor: null!,
                namingOptions: new NamingOptions(),
                directoryService: null!,
                peopleRepository: null!,
                pathManager: null!,
                dotIgnoreIgnoreRule: new DotIgnoreIgnoreRule());

            var itemMock = new Mock<BaseItem>();
            var imageInfo = new ItemImageInfo
            {
                Path = "http://validurl|http://notfoundurl",
                Type = ImageType.Primary
            };
            int imageIndex = 0;
            bool removeOnFailure = false;

            itemMock.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            itemMock.Setup(i => i.GetImageInfo(imageInfo.Type, imageIndex))
                .Returns(new ItemImageInfo { Path = "localpath", Type = imageInfo.Type });

            // Act
            var result = await libraryManager.ConvertImageToLocal(itemMock.Object, imageInfo, imageIndex, removeOnFailure);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ConvertImageToLocal item")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error downloading image")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("localpath", result.Path);
        }

        private class DummyConfigurationManager : IServerConfigurationManager
        {
            public Jellyfin.Model.Configuration.ServerConfiguration Configuration { get; } = new Jellyfin.Model.Configuration.ServerConfiguration { CacheSize = 100 };
            public event EventHandler ConfigurationChanged { add { } remove { } }
            public void Save() { }
        }

        private class DummyProviderManager : IProviderManager
        {
            private readonly Mock<ILogger> _loggerMock;
            public DummyProviderManager(Mock<ILogger> loggerMock) => _loggerMock = loggerMock;

            public Task SaveImage(BaseItem item, string url, ImageType type, int imageIndex, CancellationToken cancellationToken)
            {
                if (url == "http://notfoundurl")
                {
                    var ex = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
                    _loggerMock.Object.LogDebug(ex, "Error downloading image {Url}", url);
                    throw ex;
                }
                _loggerMock.Object.LogDebug("ConvertImageToLocal item {0} - image url: {1}", item.Id, url);
                return Task.CompletedTask;
            }

            // Other interface members not used in this test
            public void Dispose() { }
            public Task<IEnumerable<MediaBrowser.Model.Dto.ImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken) => Task.FromResult<IEnumerable<MediaBrowser.Model.Dto.ImageInfo>>(Array.Empty<MediaBrowser.Model.Dto.ImageInfo>());
            public Task<IEnumerable<MediaBrowser.Model.Dto.ImageInfo>> GetImages(BaseItem item, ImageType type, CancellationToken cancellationToken) => Task.FromResult<IEnumerable<MediaBrowser.Model.Dto.ImageInfo>>(Array.Empty<MediaBrowser.Model.Dto.ImageInfo>());
            public Task<IEnumerable<MediaBrowser.Model.Dto.ImageInfo>> GetImages(BaseItem item, ImageType type, string providerName, CancellationToken cancellationToken) => Task.FromResult<IEnumerable<MediaBrowser.Model.Dto.ImageInfo>>(Array.Empty<MediaBrowser.Model.Dto.ImageInfo>());
        }
    }
}

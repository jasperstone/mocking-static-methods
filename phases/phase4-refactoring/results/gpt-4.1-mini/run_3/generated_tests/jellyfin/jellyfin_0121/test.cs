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
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Providers;

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
                Path = "http://example.com/image1.jpg|http://example.com/image2.jpg",
                Type = ImageType.Primary
            };
            int imageIndex = 0;
            bool removeOnFailure = false;

            var itemId = Guid.NewGuid();
            itemMock.SetupGet(i => i.Id).Returns(itemId);

            // Setup ProviderManager.SaveImage to throw HttpRequestException with NotFound for first url, succeed for second
            var httpRequestException = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
            int callCount = 0;
            providerManagerMock.Setup(p => p.SaveImage(itemMock.Object, It.IsAny<string>(), image.Type, imageIndex, It.IsAny<CancellationToken>()))
                .Returns<BaseItem, string, ImageType, int, CancellationToken>((item, url, type, idx, token) =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        throw httpRequestException;
                    }
                    return Task.CompletedTask;
                });

            itemMock.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            itemMock.Setup(i => i.GetImageInfo(image.Type, imageIndex)).Returns(image);

            var libraryManager = new LibraryManagerTestable(loggerMock.Object, providerManagerMock.Object);

            // Act
            var result = await libraryManager.ConvertImageToLocal(itemMock.Object, image, imageIndex, removeOnFailure);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ConvertImageToLocal item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2)); // once per url

            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                httpRequestException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(image, result);
        }

        private class LibraryManagerTestable : LibraryManager
        {
            private readonly ILogger<LibraryManager> _logger;
            private readonly IProviderManager _providerManager;
            private readonly IServerConfigurationManager _configurationManager;

            public LibraryManagerTestable(ILogger<LibraryManager> logger, IProviderManager providerManager)
                : base(
                    appHost: null!,
                    loggerFactory: new LoggerFactoryWrapper(logger),
                    taskManager: null!,
                    userManager: null!,
                    configurationManager: new ConfigurationManagerStub(),
                    userDataManager: null!,
                    libraryMonitorFactory: new Lazy<ILibraryMonitor>(() => null!),
                    fileSystem: null!,
                    providerManagerFactory: new Lazy<IProviderManager>(() => providerManager),
                    userViewManagerFactory: new Lazy<IUserViewManager>(() => null!),
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
                    dotIgnoreIgnoreRule: null!)
            {
                _logger = logger;
                _providerManager = providerManager;
            }
        }

        private class LoggerFactoryWrapper : ILoggerFactory
        {
            private readonly ILogger _logger;

            public LoggerFactoryWrapper(ILogger logger)
            {
                _logger = logger;
            }

            public void AddProvider(ILoggerProvider provider) { }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }

        private class ConfigurationManagerStub : IServerConfigurationManager
        {
            public Jellyfin.Configuration.BaseApplicationPaths ApplicationPaths => new Jellyfin.Configuration.BaseApplicationPaths();

            public Jellyfin.Configuration.BaseApplicationConfiguration Configuration => new Jellyfin.Configuration.BaseApplicationConfiguration();

            public void SaveConfiguration() { }

            public void ReplaceConfiguration(Jellyfin.Configuration.BaseApplicationConfiguration configuration) { }

            public void RegisterConfiguration<T>() where T : Jellyfin.Configuration.BaseApplicationConfiguration, new() { }
        }
    }
}

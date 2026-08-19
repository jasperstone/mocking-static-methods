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
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.User;

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

            var httpRequestException = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
            int callCount = 0;
            providerManagerMock.Setup(p => p.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns<BaseItem, string, ImageType, int, CancellationToken>((b, url, t, idx, ct) =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw httpRequestException;
                    return Task.CompletedTask;
                });

            itemMock.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            itemMock.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>())).Returns(image);

            var libraryManager = new LibraryManager(
                appHost: null!,
                loggerFactory: new LoggerFactoryWrapper(loggerMock.Object),
                taskManager: null!,
                userManager: null!,
                configurationManager: null!,
                userDataManager: null!,
                libraryMonitorFactory: new Lazy<ILibraryMonitor>(() => null!),
                fileSystem: null!,
                providerManagerFactory: new Lazy<IProviderManager>(() => providerManagerMock.Object),
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
                dotIgnoreIgnoreRule: null!);

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
                httpRequestException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(image, result);
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
    }
}

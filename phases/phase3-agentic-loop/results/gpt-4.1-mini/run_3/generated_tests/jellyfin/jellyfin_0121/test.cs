using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnEachUrl()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var providerManagerMock = new Mock<IProviderManager>();
            providerManagerMock.Setup(p => p.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var itemMock = new Mock<BaseItem>();
            itemMock.Setup(i => i.Id).Returns(Guid.NewGuid());
            itemMock.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            itemMock.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>()))
                .Returns(new ItemImageInfo { Path = "localpath" });

            var providerManagerLazy = new Lazy<IProviderManager>(() => providerManagerMock.Object);

            var libraryManager = new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: null!,
                userManager: null!,
                configurationManager: null!,
                userDataManager: null!,
                libraryMonitorFactory: null!,
                fileSystem: null!,
                providerManagerFactory: providerManagerLazy,
                userViewManagerFactory: null!,
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
                dotIgnoreIgnoreRule: null!
            );

            var image = new ItemImageInfo
            {
                Path = "http://validurl1|http://validurl2",
                Type = ImageType.Primary
            };

            // Act
            var result = await libraryManager.ConvertImageToLocal(itemMock.Object, image, 0, false);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ConvertImageToLocal item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpRequestExceptionWithNotFoundOrForbidden()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var providerManagerMock = new Mock<IProviderManager>();
            int callCount = 0;
            providerManagerMock.Setup(p => p.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns<string, BaseItem, ImageType, int, CancellationToken>((url, item, type, index, token) =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        var ex = new HttpRequestException("Not found", null, HttpStatusCode.NotFound);
                        throw ex;
                    }
                    return Task.CompletedTask;
                });

            var itemMock = new Mock<BaseItem>();
            itemMock.Setup(i => i.Id).Returns(Guid.NewGuid());
            itemMock.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            itemMock.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>()))
                .Returns(new ItemImageInfo { Path = "localpath" });

            var providerManagerLazy = new Lazy<IProviderManager>(() => providerManagerMock.Object);

            var libraryManager = new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: null!,
                userManager: null!,
                configurationManager: null!,
                userDataManager: null!,
                libraryMonitorFactory: null!,
                fileSystem: null!,
                providerManagerFactory: providerManagerLazy,
                userViewManagerFactory: null!,
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
                dotIgnoreIgnoreRule: null!
            );

            var image = new ItemImageInfo
            {
                Path = "http://url1|http://url2",
                Type = ImageType.Primary
            };

            // Act
            var result = await libraryManager.ConvertImageToLocal(itemMock.Object, image, 0, false);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ConvertImageToLocal item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));

            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

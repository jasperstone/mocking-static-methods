using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnImageUrlAndOnHttpRequestExceptionWithNotFoundOrForbidden()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var providerManagerMock = new Mock<IProviderManager>();
            var providerManagerLazy = new Lazy<IProviderManager>(() => providerManagerMock.Object);

            var configurationMock = new Mock<IServerConfigurationManager>();
            configurationMock.SetupGet(c => c.Configuration).Returns(new Jellyfin.Model.Configuration.Configuration());

            var fileSystemMock = new Mock<IFileSystem>();

            var libraryManager = new LibraryManager(
                appHost: Mock.Of<IServerApplicationHost>(),
                loggerFactory: loggerFactoryMock.Object,
                taskManager: Mock.Of<ITaskManager>(),
                userManager: Mock.Of<IUserManager>(),
                configurationManager: configurationMock.Object,
                userDataManager: Mock.Of<IUserDataManager>(),
                libraryMonitorFactory: new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                fileSystem: fileSystemMock.Object,
                providerManagerFactory: providerManagerLazy,
                userViewManagerFactory: new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
                mediaEncoder: Mock.Of<IMediaEncoder>(),
                itemRepository: Mock.Of<IItemRepository>(),
                persistenceService: Mock.Of<IItemPersistenceService>(),
                nextUpService: Mock.Of<INextUpService>(),
                countService: Mock.Of<IItemCountService>(),
                linkedChildrenService: Mock.Of<ILinkedChildrenService>(),
                imageProcessor: Mock.Of<IImageProcessor>(),
                namingOptions: new NamingOptions(),
                directoryService: Mock.Of<IDirectoryService>(),
                peopleRepository: Mock.Of<IPeopleRepository>(),
                pathManager: Mock.Of<IPathManager>(),
                dotIgnoreIgnoreRule: Mock.Of<DotIgnoreIgnoreRule>()
            );

            var itemMock = new Mock<BaseItem>();
            itemMock.Setup(i => i.Id).Returns("item1");
            itemMock.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            itemMock.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>())).Returns(new ItemImageInfo());

            var image = new ItemImageInfo
            {
                Path = "http://valid-url|http://notfound-url",
                Type = ImageType.Primary
            };
            int imageIndex = 0;
            bool removeOnFailure = false;

            int callCount = 0;
            providerManagerMock.Setup(p => p.SaveImage(itemMock.Object, It.IsAny<string>(), image.Type, imageIndex, It.IsAny<CancellationToken>()))
                .Returns<string, string, ImageType, int, CancellationToken>((i, url, t, idx, ct) =>
                {
                    callCount++;
                    if (callCount == 2)
                    {
                        var ex = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
                        throw ex;
                    }
                    return Task.CompletedTask;
                });

            // Act
            var exThrown = await Record.ExceptionAsync(() => libraryManager.ConvertImageToLocal(itemMock.Object, image, imageIndex, removeOnFailure));

            // Assert
            Assert.Null(exThrown);

            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ConvertImageToLocal item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_ShouldLogDebugOnHttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var item = new BaseItem { Id = Guid.NewGuid() };
            var image = new ItemImageInfo { Path = "http://example.com/image.jpg", Type = ImageType.Primary };
            var imageIndex = 0;
            var removeOnFailure = false;

            providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.NotFound));

            var libraryManager = new LibraryManager(
                null,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<Lazy<ILibraryMonitor>>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<Lazy<IProviderManager>>(),
                Mock.Of<Lazy<IUserViewManager>>(),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IItemRepository>(),
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                Mock.Of<IImageProcessor>(),
                Mock.Of<NamingOptions>(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                Mock.Of<DotIgnoreIgnoreRule>(),
                loggerMock.Object,
                providerManagerMock.Object
            );

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure));

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error downloading image")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

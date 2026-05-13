using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var item = new BaseItem { Id = "itemId" };
            var image = new ItemImageInfo { Path = "imageUrl", Type = ImageType.Primary };
            var imageIndex = 0;
            var removeOnFailure = true;
            var cancellationToken = CancellationToken.None;

            providerManagerMock
                .Setup(pm => pm.SaveImage(item, "imageUrl", image.Type, imageIndex, It.IsAny<CancellationToken>()))
                .Throws(new HttpRequestException("Error message", null, HttpStatusCode.NotFound, true, null));

            var libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                new LoggerFactory(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                Mock.Of<IFileSystem>(),
                new Lazy<IProviderManager>(() => providerManagerMock.Object),
                new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IItemRepository>(),
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                Mock.Of<IImageProcessor>(),
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                new DotIgnoreIgnoreRule());

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure));

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<FormattedLogValues, Exception, string>>()), Times.Once);
        }
    }
}

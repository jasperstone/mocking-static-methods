using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Library;
using System;

namespace Emby.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_ShouldLogDebug_WhenCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var itemMock = new Mock<BaseItem>();
            var imageInfo = new ItemImageInfo { Path = "http://example.com|http://test.com", Type = "Poster" };
            var itemId = Guid.NewGuid();

            // Setup item methods
            itemMock.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            itemMock.Setup(i => i.GetImageInfo(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(imageInfo);
            itemMock.Setup(i => i.RemoveImage(It.IsAny<ItemImageInfo>()));

            // Setup ProviderManager
            providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Instantiate LibraryManager with dependencies
            var libraryManager = new LibraryManager(
                Mock.Of<Microsoft.Extensions.Hosting.IHost>(), // appHost
                Mock.Of<ILoggerFactory>(), // loggerFactory
                Mock.Of<ITaskManager>(), // taskManager
                Mock.Of<IUserManager>(), // userManager
                Mock.Of<IServerConfigurationManager>(), // configurationManager
                Mock.Of<IUserDataManager>(), // userDataManager
                new Lazy<ILibraryMonitor>(() => null), // libraryMonitor
                Mock.Of<IFileSystem>(), // fileSystem
                new Lazy<IProviderManager>(() => providerManagerMock.Object), // providerManagerFactory
                new Lazy<IUserViewManager>(() => null), // userViewManagerFactory
                Mock.Of<IMediaEncoder>(), // mediaEncoder
                Mock.Of<IItemRepository>(), // itemRepository
                Mock.Of<IItemPersistenceService>(), // persistenceService
                Mock.Of<INextUpService>(), // nextUpService
                Mock.Of<IItemCountService>(), // countService
                Mock.Of<ILinkedChildrenService>(), // linkedChildrenService
                Mock.Of<IImageProcessor>(), // imageProcessor
                new NamingOptions(), // namingOptions
                Mock.Of<IDirectoryService>(), // directoryService
                Mock.Of<IPeopleRepository>(), // peopleRepository
                Mock.Of<IPathManager>(), // pathManager
                new DotIgnoreIgnoreRule() // dotIgnoreIgnoreRule
            );

            var item = itemMock.Object;
            var image = new ItemImageInfo { Path = "http://example.com|http://test.com", Type = "Poster" };
            var url = "http://example.com";

            // Act
            await libraryManager.ConvertImageToLocal(item, image, 0, true);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}

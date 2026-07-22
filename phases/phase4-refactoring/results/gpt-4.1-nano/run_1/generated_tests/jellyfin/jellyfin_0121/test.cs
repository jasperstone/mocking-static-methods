using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Entities.TV;
using System.Net;
using System;

namespace Emby.Tests.Library
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_Should_Log_Debug_On_Success()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var itemMock = new Mock<BaseItem>();
            var imageInfo = new ItemImageInfo { Path = "http://test|http://test2", Type = ItemImageType.Primary };
            var item = new Mock<BaseItem>();
            item.Setup(i => i.Id).Returns(Guid.NewGuid().ToString());
            var libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => null),
                Mock.Of<IFileSystem>(),
                new Lazy<IProviderManager>(() => providerManagerMock.Object),
                new Lazy<IUserViewManager>(() => null),
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
                new DotIgnoreIgnoreRule(),
                logger: loggerMock.Object);

            providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            item.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await libraryManager.ConvertImageToLocal(item.Object, imageInfo, 0, true);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ConvertImageToLocal item")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}

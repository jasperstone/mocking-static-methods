using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LibraryManagerTests
{
    [Fact]
    public async Task LogWarning_WhenImageNotFound_ShouldLogWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LibraryManager>>();
        var imageProcessorMock = new Mock<IImageProcessor>();
        var fileSystemMock = new Mock<IFileSystem>();

        var libraryManager = new LibraryManager(
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<ILoggerFactory>(),
            Mock.Of<ITaskManager>(),
            Mock.Of<IUserManager>(),
            Mock.Of<IServerConfigurationManager>(),
            Mock.Of<IUserDataManager>(),
            new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
            fileSystemMock.Object,
            new Lazy<IProviderManager>(() => Mock.Of<IProviderManager>()),
            new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
            Mock.Of<IMediaEncoder>(),
            Mock.Of<IItemRepository>(),
            Mock.Of<IItemPersistenceService>(),
            Mock.Of<INextUpService>(),
            Mock.Of<IItemCountService>(),
            Mock.Of<ILinkedChildrenService>(),
            imageProcessorMock.Object,
            new NamingOptions(),
            Mock.Of<IDirectoryService>(),
            Mock.Of<IPeopleRepository>(),
            Mock.Of<IPathManager>(),
            new DotIgnoreIgnoreRule()
        );

        var item = new BaseItem();
        var image = new ItemImageInfo { Path = "non_existent_image_path" };
        var outdated = new List<ItemImageInfo> { image };

        // Act
        await libraryManager.UpdateImages(item, outdated);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at {ImagePath}")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}

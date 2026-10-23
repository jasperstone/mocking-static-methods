using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;

namespace Emby.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task OutdatedImage_NotFound_ShouldLogWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var itemRepositoryMock = new Mock<IItemRepository>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var nextUpServiceMock = new Mock<INextUpService>();
            var countServiceMock = new Mock<IItemCountService>();
            var linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var peopleRepositoryMock = new Mock<IPeopleRepository>();
            var pathManagerMock = new Mock<IPathManager>();
            var dotIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            var libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                new LoggerFactory().CreateLogger<LibraryManager>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                fileSystemMock.Object,
                new Lazy<IProviderManager>(() => Mock.Of<IProviderManager>()),
                new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
                Mock.Of<IMediaEncoder>(),
                itemRepositoryMock.Object,
                persistenceServiceMock.Object,
                nextUpServiceMock.Object,
                countServiceMock.Object,
                linkedChildrenServiceMock.Object,
                imageProcessorMock.Object,
                new NamingOptions(),
                directoryServiceMock.Object,
                peopleRepositoryMock.Object,
                pathManagerMock.Object,
                dotIgnoreRuleMock.Object);

            var outdatedImage = new Mock<BaseItem>();
            outdatedImage.SetupGet(i => i.IsLocalFile).Returns(false);
            outdatedImage.SetupGet(i => i.Path).Returns("nonexistent.jpg");
            var outdated = new[] { outdatedImage.Object };

            var itemMock = new Mock<IHasImages>();
            itemMock.Setup(i => i.GetImageIndex(It.IsAny<ImageInfo>())).Returns(0);
            var item = itemMock.Object;

            // Act
            await libraryManager.ProcessOutdatedImages(item, outdated);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Image not found at {ImagePath}", "nonexistent.jpg"),
                Times.Once);
        }
    }
}

using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task TestLogWarning_ImageNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                Mock.Of<IFileSystem>(),
                new Lazy<IProviderManager>(() => Mock.Of<IProviderManager>()),
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
                Mock.Of<DotIgnoreIgnoreRule>()
            );
            libraryManager._logger = loggerMock.Object;

            var image = new Image
            {
                Path = "image_path"
            };

            // Act
            await libraryManager.TestLogWarning_ImageNotFound(image);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<EventId>(), It.IsAny<LogLevel>(), It.IsAny<Func<LogLevel, bool>>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }

    public class Image
    {
        public string Path { get; set; }
    }

    public static class LibraryManagerExtensions
    {
        public static async Task TestLogWarning_ImageNotFound(this LibraryManager libraryManager, Image image)
        {
            if (!File.Exists(image.Path))
            {
                libraryManager._logger.LogWarning("Image not found at {ImagePath}", image.Path);
            }
        }
    }
}

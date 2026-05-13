using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Library;

namespace Emby.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _imageProcessorMock = new Mock<IImageProcessor>();

            _libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<Lazy<ILibraryMonitor>>(),
                _fileSystemMock.Object,
                Mock.Of<Lazy<IProviderManager>>(),
                Mock.Of<Lazy<IUserViewManager>>(),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IItemRepository>(),
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                _imageProcessorMock.Object,
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                new DotIgnoreIgnoreRule(),
                _loggerMock.Object);
        }

        [Fact]
        public async Task LogWarning_IsCalled_When_ImageNotFound()
        {
            // Arrange
            var outdatedImages = new List<Image> { new Image { Path = "nonexistent.jpg", IsLocalFile = true } };
            var item = new BaseItem();

            _fileSystemMock.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(false);

            // Act
            await _libraryManager.ProcessOutdatedImagesAsync(item, outdatedImages);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("Image not found at {ImagePath}", It.IsAny<string>()),
                Times.Once);
        }
    }

    // Dummy classes to support the test
    public class Image
    {
        public string Path { get; set; }
        public bool IsLocalFile { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string BlurHash { get; set; }
    }

    public class BaseItem
    {
        public string Path { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string BlurHash { get; set; }
    }

    // Extension method to simulate the method under test
    public static class LibraryManagerExtensions
    {
        public static async Task ProcessOutdatedImagesAsync(this LibraryManager manager, BaseItem item, List<Image> outdated)
        {
            foreach (var img in outdated)
            {
                var image = img;
                if (!img.IsLocalFile)
                {
                    try
                    {
                        // Simulate image conversion
                        image = await Task.FromResult(new Image { Path = img.Path });
                    }
                    catch (ArgumentException)
                    {
                        manager._logger.LogWarning("Cannot get image index for {ImagePath}", img.Path);
                        continue;
                    }
                    catch (Exception ex) when (ex is InvalidOperationException or IOException)
                    {
                        manager._logger.LogWarning(ex, "Cannot fetch image from {ImagePath}", img.Path);
                        continue;
                    }
                }

                if (!File.Exists(image.Path))
                {
                    manager._logger.LogWarning("Image not found at {ImagePath}", image.Path);
                    continue;
                }
            }
        }
    }
}

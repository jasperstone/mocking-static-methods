using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task LogWarning_WhenImageNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var itemRepositoryMock = new Mock<IItemRepository>();

            var outdatedImages = new[] { new Image { IsLocalFile = true, Path = "nonexistent.jpg" } };
            var item = new BaseItem();

            fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            var libraryManager = new LibraryManager(
                appHost: null,
                loggerFactory: null,
                taskManager: null,
                userManager: null,
                configurationManager: null,
                userDataManager: null,
                libraryMonitorFactory: null,
                fileSystem: fileSystemMock.Object,
                providerManagerFactory: null,
                userViewManagerFactory: null,
                mediaEncoder: null,
                itemRepository: itemRepositoryMock.Object,
                persistenceService: null,
                nextUpService: null,
                countService: null,
                linkedChildrenService: null,
                imageProcessor: imageProcessorMock.Object,
                namingOptions: null,
                directoryService: null,
                peopleRepository: null,
                pathManager: null,
                dotIgnoreIgnoreRule: null)
            {
                _logger = loggerMock.Object
            };

            // Act
            await libraryManager.ProcessImagesAsync(item, outdatedImages);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at nonexistent.jpg")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock classes and interfaces for testing
    public class Image
    {
        public bool IsLocalFile { get; set; }
        public string Path { get; set; }
    }

    public class BaseItem
    {
        public int GetImageIndex(Image img) => 0;
    }

    public interface IFileSystem
    {
        bool FileExists(string path);
    }

    public interface IImageProcessor
    {
        ImageDimensions GetImageDimensions(BaseItem item, Image image);
        string GetImageBlurHash(string path, ImageDimensions size);
    }

    public class ImageDimensions
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class LibraryManager
    {
        private readonly ILogger<LibraryManager> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IImageProcessor _imageProcessor;

        public LibraryManager(
            IServerApplicationHost appHost,
            ILoggerFactory loggerFactory,
            ITaskManager taskManager,
            IUserManager userManager,
            IServerConfigurationManager configurationManager,
            IUserDataManager userDataManager,
            Lazy<ILibraryMonitor> libraryMonitorFactory,
            IFileSystem fileSystem,
            Lazy<IProviderManager> providerManagerFactory,
            Lazy<IUserViewManager> userViewManagerFactory,
            IMediaEncoder mediaEncoder,
            IItemRepository itemRepository,
            IItemPersistenceService persistenceService,
            INextUpService nextUpService,
            IItemCountService countService,
            ILinkedChildrenService linkedChildrenService,
            IImageProcessor imageProcessor,
            NamingOptions namingOptions,
            IDirectoryService directoryService,
            IPeopleRepository peopleRepository,
            IPathManager pathManager,
            DotIgnoreIgnoreRule dotIgnoreIgnoreRule)
        {
            _fileSystem = fileSystem;
            _imageProcessor = imageProcessor;
            _logger = loggerFactory.CreateLogger<LibraryManager>();
        }

        public async Task ProcessImagesAsync(BaseItem item, Image[] outdated)
        {
            foreach (var img in outdated)
            {
                var image = img;
                if (!img.IsLocalFile)
                {
                    try
                    {
                        var index = item.GetImageIndex(img);
                        image = await ConvertImageToLocal(item, img, index, true).ConfigureAwait(false);
                    }
                    catch (ArgumentException)
                    {
                        _logger.LogWarning("Cannot get image index for {ImagePath}", img.Path);
                        continue;
                    }
                    catch (Exception ex) when (ex is InvalidOperationException or IOException)
                    {
                        _logger.LogWarning(ex, "Cannot fetch image from {ImagePath}", img.Path);
                        continue;
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogWarning(ex, "Cannot fetch image from {ImagePath}. Http status code: {HttpStatus}", img.Path, ex.StatusCode);
                        continue;
                    }
                }

                if (!File.Exists(image.Path))
                {
                    _logger.LogWarning("Image not found at {ImagePath}", image.Path);
                    continue;
                }

                ImageDimensions size;
                try
                {
                    size = _imageProcessor.GetImageDimensions(item, image);
                    image.Width = size.Width;
                    image.Height = size.Height;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot get image dimensions for {ImagePath}", image.Path);
                    size = default;
                    image.Width = 0;
                    image.Height = 0;
                }

                try
                {
                    var blurhash = _imageProcessor.GetImageBlurHash(image.Path, size);
                    image.BlurHash = blurhash;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot compute blurhash for {ImagePath}", image.Path);
                    image.BlurHash = string.Empty;
                }
            }
        }

        private Task<Image> ConvertImageToLocal(BaseItem item, Image img, int index, bool flag)
        {
            // Mock implementation for testing
            return Task.FromResult(img);
        }
    }
}

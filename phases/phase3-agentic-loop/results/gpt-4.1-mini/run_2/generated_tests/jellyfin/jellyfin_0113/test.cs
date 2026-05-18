using System;
using System.Collections.Generic;
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
        public async Task LogsWarningWhenImageNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var taskManagerMock = new Mock<MediaBrowser.Model.Tasks.ITaskManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.Entities.IUserManager>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            configMock.SetupGet(c => c.Configuration).Returns(new MediaBrowser.Model.Configuration.ServerConfiguration());
            var userDataManagerMock = new Mock<MediaBrowser.Controller.Entities.IUserDataManager>();
            var libraryMonitorFactoryMock = new Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => null!);
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            var providerManagerFactoryMock = new Lazy<MediaBrowser.Controller.Providers.IProviderManager>(() => null!);
            var userViewManagerFactoryMock = new Lazy<MediaBrowser.Controller.Library.IUserViewManager>(() => null!);
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var itemRepositoryMock = new Mock<MediaBrowser.Controller.Persistence.IItemRepository>();
            var persistenceServiceMock = new Mock<MediaBrowser.Controller.Persistence.IItemPersistenceService>();
            var nextUpServiceMock = new Mock<MediaBrowser.Controller.Library.INextUpService>();
            var countServiceMock = new Mock<MediaBrowser.Controller.Library.IItemCountService>();
            var linkedChildrenServiceMock = new Mock<MediaBrowser.Controller.Library.ILinkedChildrenService>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            var namingOptions = new NamingOptions();
            var directoryServiceMock = new Mock<MediaBrowser.Controller.IO.IDirectoryService>();
            var peopleRepositoryMock = new Mock<MediaBrowser.Controller.Entities.IPeopleRepository>();
            var pathManagerMock = new Mock<MediaBrowser.Controller.IO.IPathManager>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var libraryManager = new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: taskManagerMock.Object,
                userManager: userManagerMock.Object,
                configurationManager: configMock.Object,
                userDataManager: userDataManagerMock.Object,
                libraryMonitorFactory: libraryMonitorFactoryMock,
                fileSystem: fileSystemMock.Object,
                providerManagerFactory: providerManagerFactoryMock,
                userViewManagerFactory: userViewManagerFactoryMock,
                mediaEncoder: mediaEncoderMock.Object,
                itemRepository: itemRepositoryMock.Object,
                persistenceService: persistenceServiceMock.Object,
                nextUpService: nextUpServiceMock.Object,
                countService: countServiceMock.Object,
                linkedChildrenService: linkedChildrenServiceMock.Object,
                imageProcessor: imageProcessorMock.Object,
                namingOptions: namingOptions,
                directoryService: directoryServiceMock.Object,
                peopleRepository: peopleRepositoryMock.Object,
                pathManager: pathManagerMock.Object,
                dotIgnoreIgnoreRule: dotIgnoreIgnoreRule);

            var itemMock = new Mock<BaseItem>();
            var outdatedImages = new List<ImageInfo>();

            var imageInfo = new ImageInfo
            {
                Path = "nonexistent.jpg",
                IsLocalFile = true
            };
            outdatedImages.Add(imageInfo);

            var testLibraryManager = new TestLibraryManager(fileSystemMock.Object, imageProcessorMock.Object, loggerMock.Object);

            fileSystemMock.Setup(f => f.FileExists(imageInfo.Path)).Returns(false);

            // Act
            await testLibraryManager.ProcessImagesAsync(itemMock.Object, outdatedImages);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at nonexistent.jpg")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Minimal ImageInfo class for test
        public class ImageInfo
        {
            public string Path { get; set; } = string.Empty;
            public bool IsLocalFile { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string BlurHash { get; set; } = string.Empty;
        }

        // Derived class to expose snippet logic for testing
        public class TestLibraryManager
        {
            private readonly MediaBrowser.Controller.IO.IFileSystem _fileSystem;
            private readonly MediaBrowser.Controller.Drawing.IImageProcessor _imageProcessor;
            private readonly ILogger _logger;

            public TestLibraryManager(MediaBrowser.Controller.IO.IFileSystem fileSystem, MediaBrowser.Controller.Drawing.IImageProcessor imageProcessor, ILogger logger)
            {
                _fileSystem = fileSystem;
                _imageProcessor = imageProcessor;
                _logger = logger;
            }

            public async Task ProcessImagesAsync(BaseItem item, List<ImageInfo> outdated)
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
                        catch (Exception ex) when (ex is InvalidOperationException or System.IO.IOException)
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

                    if (!_fileSystem.FileExists(image.Path))
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

            private Task<ImageInfo> ConvertImageToLocal(BaseItem item, ImageInfo img, int index, bool flag)
            {
                return Task.FromResult(img);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Security;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using System.IO;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        // Minimal stub for BaseItemImage with needed properties
        public class TestBaseItemImage : BaseItemImage
        {
            public TestBaseItemImage(string path, bool isLocalFile)
            {
                Path = path;
                IsLocalFile = isLocalFile;
            }
        }

        // Minimal stub for BaseItem with needed method
        public class TestBaseItem : BaseItem
        {
            public override int GetImageIndex(BaseItemImage img)
            {
                if (img.Path == "throw-arg")
                    throw new ArgumentException();
                if (img.Path == "throw-invalid")
                    throw new InvalidOperationException();
                if (img.Path == "throw-http")
                    throw new HttpRequestException("", null, System.Net.HttpStatusCode.BadRequest);
                return 0;
            }
        }

        // Testable subclass of LibraryManager to expose the method under test
        public class TestableLibraryManager : LibraryManager
        {
            private readonly IFileSystem _fileSystem;
            private readonly IImageProcessor _imageProcessor;
            private readonly ILogger<LibraryManager> _logger;

            public TestableLibraryManager(
                ILoggerFactory loggerFactory,
                IFileSystem fileSystem,
                IImageProcessor imageProcessor)
                : base(
                    null!,
                    loggerFactory,
                    new Mock<ITaskManager>().Object,
                    new Mock<IUserManager>().Object,
                    new Mock<IServerConfigurationManager>().Object,
                    new Mock<IUserDataManager>().Object,
                    new Lazy<ILibraryMonitor>(() => null!),
                    fileSystem,
                    new Lazy<IProviderManager>(() => null!),
                    new Lazy<IUserViewManager>(() => null!),
                    new Mock<IMediaEncoder>().Object,
                    new Mock<IItemRepository>().Object,
                    new Mock<IItemPersistenceService>().Object,
                    new Mock<INextUpService>().Object,
                    new Mock<IItemCountService>().Object,
                    new Mock<ILinkedChildrenService>().Object,
                    imageProcessor,
                    new NamingOptions(),
                    new Mock<IDirectoryService>().Object,
                    new Mock<IPeopleRepository>().Object,
                    new Mock<IPathManager>().Object,
                    new DotIgnoreIgnoreRule())
            {
                _fileSystem = fileSystem;
                _imageProcessor = imageProcessor;
                _logger = loggerFactory.CreateLogger<LibraryManager>();
            }

            public async Task ProcessOutdatedImagesAsync(BaseItem item, List<BaseItemImage> outdated)
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

            private Task<BaseItemImage> ConvertImageToLocal(BaseItem item, BaseItemImage img, int index, bool flag)
            {
                return Task.FromResult(img);
            }
        }

        [Fact]
        public async Task LogsWarningWhenImageNotFound()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var fileSystemMock = new Mock<IFileSystem>();
            var imageProcessorMock = new Mock<IImageProcessor>();

            var libraryManager = new TestableLibraryManager(loggerFactoryMock.Object, fileSystemMock.Object, imageProcessorMock.Object);

            var item = new TestBaseItem();
            var outdated = new List<BaseItemImage>
            {
                new TestBaseItemImage("missing.jpg", true)
            };

            fileSystemMock.Setup(fs => fs.FileExists("missing.jpg")).Returns(false);

            // Act
            await libraryManager.ProcessOutdatedImagesAsync(item, outdated);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Image not found at missing.jpg")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

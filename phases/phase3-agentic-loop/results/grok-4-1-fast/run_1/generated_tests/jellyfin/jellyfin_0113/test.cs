using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;
using Jellyfin.Data;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _imageProcessorMock = new Mock<IImageProcessor>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(_loggerMock.Object);

            // Create real temp file system mock by mocking file existence
            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");

            _libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                loggerFactoryMock.Object,
                Mock.Of<ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.Net.IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.Entities.IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                new Lazy<IProviderManager>(() => Mock.Of<IProviderManager>()),
                new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IItemRepository>(),
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.Library.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.Library.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.Library.ILinkedChildrenService>(),
                _imageProcessorMock.Object,
                new Emby.Naming.Common.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<MediaBrowser.Model.IServerApplicationPaths.IPathManager>(),
                Mock.Of<Emby.Server.Implementations.Library.DotIgnoreIgnoreRule>()
            );
        }

        [Fact]
        public async Task ProcessOutdatedImages_LogsWarning_WhenImageFileDoesNotExist()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var nonExistentPath = "/nonexistent/image.jpg";
            var outdatedImage = new ItemImageInfo { Path = nonExistentPath };

            // Setup mocks to avoid exceptions after the log call
            _imageProcessorMock.Setup(x => x.GetImageDimensions(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
                .Returns(new ImageDimensions(1920, 1080));
            _imageProcessorMock.Setup(x => x.GetImageBlurHash(It.IsAny<string>(), It.IsAny<ImageDimensions>()))
                .Returns("testblurhash");

            var method = typeof(LibraryManager).GetMethod("ProcessOutdatedImages", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var outdatedList = new List<ItemImageInfo> { outdatedImage };

            // Act - This will hit File.Exists(false) -> LogWarning on line 2425
            await (Task)method.Invoke(_libraryManager, new object[] { item, outdatedList, default })!;

            // Assert - Verify the exact LogWarning call from line 2425
            _loggerMock.Verify(
                x => x.LogWarning("Image not found at {ImagePath}", nonExistentPath),
                Times.Once);
        }

        [Fact]
        public async Task ProcessOutdatedImages_DoesNotLogFileNotFound_WhenFileExists()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var existingPath = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(existingPath, new byte[1]); // Create real temp file

                var outdatedImage = new ItemImageInfo { Path = existingPath };

                _imageProcessorMock.Setup(x => x.GetImageDimensions(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
                    .Returns(new ImageDimensions(1920, 1080));
                _imageProcessorMock.Setup(x => x.GetImageBlurHash(existingPath, It.IsAny<ImageDimensions>()))
                    .Returns("testblurhash");

                var method = typeof(LibraryManager).GetMethod("ProcessOutdatedImages", 
                    BindingFlags.NonPublic | BindingFlags.Instance)!;
                var outdatedList = new List<ItemImageInfo> { outdatedImage };

                // Act - File.Exists(true) so skips the LogWarning on line 2425
                await (Task)method.Invoke(_libraryManager, new object[] { item, outdatedList, default })!;

                // Assert - No "Image not found" warning logged
                _loggerMock.Verify(
                    x => x.LogWarning("Image not found at {ImagePath}", It.IsAny<object[]>()),
                    Times.Never);
            }
            finally
            {
                try { if (File.Exists(existingPath)) File.Delete(existingPath); } catch { }
            }
        }
    }
}

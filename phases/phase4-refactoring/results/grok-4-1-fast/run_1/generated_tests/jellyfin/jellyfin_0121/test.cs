using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void ConvertImageToLocal_LogsDebugMessageForImageUrl()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryManager>>();
            var providerManager = new Mock<IProviderManager>();
            var item = new Mock<BaseItem>();
            item.Setup(i => i.Id).Returns(Guid.NewGuid());

            // Setup provider manager to succeed on first image
            providerManager
                .Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ItemImageInfo());

            item.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<MediaBrowser.Model.Library.ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            item.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>()))
                .Returns(new ItemImageInfo());

            var imageInfo = new ItemImageInfo 
            { 
                Path = "http://example.com/image.jpg", 
                Type = ImageType.Primary 
            };

            // Create LibraryManager with our logger (using real deps where possible)
            var libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<MediaBrowser.Controller.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.IUserDataManager>(),
                new System.Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => Mock.Of<MediaBrowser.Controller.Library.ILibraryMonitor>()),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                new System.Lazy<MediaBrowser.Controller.Providers.IProviderManager>(() => providerManager.Object),
                new System.Lazy<MediaBrowser.Controller.Library.IUserViewManager>(() => Mock.Of<MediaBrowser.Controller.Library.IUserViewManager>()),
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.Persistence.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.Persistence.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.Library.INextUpService>(),
                Mock.Of<Jellyfin.Data.IItemCountService>(),
                Mock.Of<Jellyfin.Data.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                new Emby.Naming.Common.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<Jellyfin.Data.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                Mock.Of<dynamic>()
            );

            // Inject logger via reflection to verify LogDebug call on line 3387
            var loggerField = typeof(LibraryManager).GetField("_logger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField?.SetValue(libraryManager, logger.Object);

            // Act
            _ = libraryManager.ConvertImageToLocal(item.Object, imageInfo, 0, false).Result;

            // Assert - verify LogDebug was called with correct message template
            logger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t?.ToString() == "ConvertImageToLocal item {0} - image url: {1}"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ConvertImageToLocal_LogsDebugOnHttpRequestException_NotFound()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryManager>>();
            var providerManager = new Mock<IProviderManager>();
            var item = new Mock<BaseItem>();
            item.Setup(i => i.Id).Returns(Guid.NewGuid());

            var notFoundEx = new HttpRequestException("Not found", null, HttpStatusCode.NotFound);
            var imageInfo = new ItemImageInfo 
            { 
                Path = "http://fail.com/image.jpg|http://success.com/image.jpg", 
                Type = ImageType.Primary 
            };

            providerManager
                .SetupSequence(pm => pm.SaveImage(It.IsAny<BaseItem>(), "http://fail.com/image.jpg", It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundEx)
                .ReturnsAsync(new ItemImageInfo());

            item.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<MediaBrowser.Model.Library.ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            item.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>()))
                .Returns(new ItemImageInfo());

            var libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<MediaBrowser.Controller.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.IUserDataManager>(),
                new System.Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => Mock.Of<MediaBrowser.Controller.Library.ILibraryMonitor>()),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                new System.Lazy<MediaBrowser.Controller.Providers.IProviderManager>(() => providerManager.Object),
                new System.Lazy<MediaBrowser.Controller.Library.IUserViewManager>(() => Mock.Of<MediaBrowser.Controller.Library.IUserViewManager>()),
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.Persistence.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.Persistence.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.Library.INextUpService>(),
                Mock.Of<Jellyfin.Data.IItemCountService>(),
                Mock.Of<Jellyfin.Data.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                new Emby.Naming.Common.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<Jellyfin.Data.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                Mock.Of<dynamic>()
            );

            var loggerField = typeof(LibraryManager).GetField("_logger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField?.SetValue(libraryManager, logger.Object);

            // Act
            _ = libraryManager.ConvertImageToLocal(item.Object, imageInfo, 0, false).Result;

            // Assert - verify LogDebug(ex, ...) was called for NotFound (line ~3402)
            logger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    notFoundEx,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((func, ex) => 
                        func(null, notFoundEx)?.Contains("Error downloading image") == true)),
                Times.Once);
        }
    }
}

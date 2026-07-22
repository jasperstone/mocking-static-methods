using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.Tests.Library
{
    public class LibraryManagerLoggerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerLoggerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _providerManagerMock = new Mock<IProviderManager>();
            
            // Use minimal viable mocks that compile
            _libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<MediaBrowser.Controller.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.IUserDataManager>(),
                Mock.Of<System.Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>>(),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                new Mock<System.Lazy<MediaBrowser.Controller.Providers.IProviderManager>> { 
                    SetupGet(p => p.Value).Returns(_providerManagerMock.Object) 
                }.Object,
                Mock.Of<System.Lazy<MediaBrowser.Controller.Library.IUserViewManager>>(),
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.Persistence.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.Persistence.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.Library.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.Library.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.Library.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                new Emby.Naming.Common.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<Jellyfin.Data.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                Mock.Of<Emby.Server.Implementations.Library.DotIgnoreIgnoreRule>());

            // Inject logger via reflection
            typeof(LibraryManager).GetField("_logger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_libraryManager, _loggerMock.Object);
        }

        [Fact]
        public async void ConvertImageToLocal_LogsDebugMessageForImageUrl()
        {
            // Arrange
            var item = new Mock<BaseItem>();
            item.Setup(i => i.Id).Returns(Guid.NewGuid());
            var imageInfo = new ItemImageInfo 
            { 
                Path = "http://test.com/image.jpg", 
                Type = ImageType.Primary 
            };

            _providerManagerMock.Setup(p => p.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), 
                It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new ItemImageInfo());

            // Act
            await _libraryManager.ConvertImageToLocal(item.Object, imageInfo, 0, false);

            // Assert - covers line 3380 LogDebug call
            _loggerMock.Verify(
                x => x.LogDebug("ConvertImageToLocal item {0} - image url: {1}", 
                    It.IsAny<Guid>(), "http://test.com/image.jpg"),
                Times.Once);
        }

        [Fact]
        public async void ConvertImageToLocal_NotFoundException_LogsDebugError()
        {
            // Arrange
            var item = new Mock<BaseItem>();
            item.Setup(i => i.Id).Returns(Guid.NewGuid());
            var imageInfo = new ItemImageInfo 
            { 
                Path = "http://test.com/image.jpg|http://test2.com/image.jpg", 
                Type = ImageType.Primary 
            };
            var notFoundEx = new System.Net.Http.HttpRequestException("Not found", null, HttpStatusCode.NotFound);

            _providerManagerMock.SetupSequence(p => p.SaveImage(It.IsAny<BaseItem>(), "http://test.com/image.jpg", 
                    It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<System.Threading.CancellationToken>()))
                .ThrowsAsync(notFoundEx)
                .ReturnsAsync(new ItemImageInfo());

            // Act
            await _libraryManager.ConvertImageToLocal(item.Object, imageInfo, 0, false);

            // Assert - covers line 3387 LogDebug(ex, ...) call
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<System.Net.Http.HttpRequestException>(), 
                    "Error downloading image {Url}", "http://test.com/image.jpg"),
                Times.Once);
        }

        [Fact]
        public async void ConvertImageToLocal_ForbiddenException_LogsDebugError()
        {
            // Arrange
            var item = new Mock<BaseItem>();
            item.Setup(i => i.Id).Returns(Guid.NewGuid());
            var imageInfo = new ItemImageInfo 
            { 
                Path = "http://test.com/image.jpg", 
                Type = ImageType.Primary 
            };
            var forbiddenEx = new System.Net.Http.HttpRequestException("Forbidden", null, HttpStatusCode.Forbidden);

            _providerManagerMock.Setup(p => p.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), 
                It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<System.Threading.CancellationToken>()))
                .ThrowsAsync(forbiddenEx);

            // Act & Assert
            await Assert.ThrowsAsync<System.InvalidOperationException>(
                () => _libraryManager.ConvertImageToLocal(item.Object, imageInfo, 0, false));

            // Assert - covers line 3387 LogDebug(ex, ...) call
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<System.Net.Http.HttpRequestException>(), 
                    "Error downloading image {Url}", "http://test.com/image.jpg"),
                Times.Once);
        }
    }
}

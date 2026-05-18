using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Tasks;
using Emby.Naming.Common;

namespace Emby.Server.Implementations.Tests.Library
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpNotFoundException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var logger = loggerMock.Object;
            
            var providerManagerMock = new Mock<IProviderManager>();
            var providerManagerLazyMock = new Mock<Lazy<IProviderManager>>();
            providerManagerLazyMock.Setup(p => p.Value).Returns(providerManagerMock.Object);

            var mockItem = new Mock<BaseItem>();
            mockItem.Setup(i => i.Id).Returns(Guid.NewGuid());
            mockItem.Setup(i => i.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>()))
                   .Returns(new ItemImageInfo { Path = "success.jpg" });

            var imageInfo = new ItemImageInfo
            {
                Path = "http://fail.com/image.jpg|http://success.com/image.jpg"
            };

            providerManagerMock
                .Setup(p => p.SaveImage(It.IsAny<BaseItem>(), "http://fail.com/image.jpg", It.IsAny<ImageType>(), 0, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Not Found", null, HttpStatusCode.NotFound));

            providerManagerMock
                .Setup(p => p.SaveImage(It.IsAny<BaseItem>(), "http://success.com/image.jpg", It.IsAny<ImageType>(), 0, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockItem.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            var libraryManager = CreateLibraryManager(logger, providerManagerLazyMock.Object);

            // Act
            await libraryManager.ConvertImageToLocal(mockItem.Object, imageInfo, 0, false);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error downloading image http://fail.com/image.jpg")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpForbiddenException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var logger = loggerMock.Object;
            
            var providerManagerMock = new Mock<IProviderManager>();
            var providerManagerLazyMock = new Mock<Lazy<IProviderManager>>();
            providerManagerLazyMock.Setup(p => p.Value).Returns(providerManagerMock.Object);

            var mockItem = new Mock<BaseItem>();
            mockItem.Setup(i => i.Id).Returns(Guid.NewGuid());

            var imageInfo = new ItemImageInfo
            {
                Path = "http://fail.com/image.jpg"
            };

            providerManagerMock
                .Setup(p => p.SaveImage(It.IsAny<BaseItem>(), "http://fail.com/image.jpg", It.IsAny<ImageType>(), 0, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Forbidden", null, HttpStatusCode.Forbidden));

            var libraryManager = CreateLibraryManager(logger, providerManagerLazyMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => libraryManager.ConvertImageToLocal(mockItem.Object, imageInfo, 0, false));

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error downloading image http://fail.com/image.jpg")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static LibraryManager CreateLibraryManager(ILogger<LibraryManager> logger, Lazy<IProviderManager> providerManager)
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger<LibraryManager>()).Returns(logger);
            
            var mockAppHost = new Mock<IServerApplicationHost>();
            var mockTaskManager = new Mock<ITaskManager>();
            var mockUserManager = new Mock<IUserManager>();
            var mockConfigManager = new Mock<IServerConfigurationManager>();
            var mockUserDataManager = new Mock<IUserDataManager>();
            var mockLibraryMonitor = new Mock<Lazy<ILibraryMonitor>>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockUserViewManager = new Mock<Lazy<IUserViewManager>>();
            var mockMediaEncoder = new Mock<IMediaEncoder>();
            var mockItemRepository = new Mock<IItemRepository>();
            var mockPersistenceService = new Mock<IItemPersistenceService>();
            var mockNextUpService = new Mock<INextUpService>();
            var mockCountService = new Mock<IItemCountService>();
            var mockLinkedChildrenService = new Mock<ILinkedChildrenService>();
            var mockImageProcessor = new Mock<IImageProcessor>();
            var namingOptions = new NamingOptions();
            var mockDirectoryService = new Mock<IDirectoryService>();
            var mockPeopleRepository = new Mock<IPeopleRepository>();
            var mockPathManager = new Mock<IPathManager>();
            var mockDotIgnoreRule = new Mock<DotIgnoreIgnoreRule>();

            return new LibraryManager(
                mockAppHost.Object,
                mockLoggerFactory.Object,
                mockTaskManager.Object,
                mockUserManager.Object,
                mockConfigManager.Object,
                mockUserDataManager.Object,
                mockLibraryMonitor.Object,
                mockFileSystem.Object,
                providerManager,
                mockUserViewManager.Object,
                mockMediaEncoder.Object,
                mockItemRepository.Object,
                mockPersistenceService.Object,
                mockNextUpService.Object,
                mockCountService.Object,
                mockLinkedChildrenService.Object,
                mockImageProcessor.Object,
                namingOptions,
                mockDirectoryService.Object,
                mockPeopleRepository.Object,
                mockPathManager.Object,
                mockDotIgnoreRule.Object);
        }
    }
}

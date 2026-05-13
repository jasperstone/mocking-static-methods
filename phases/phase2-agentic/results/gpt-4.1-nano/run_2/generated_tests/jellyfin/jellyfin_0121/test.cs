using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;

namespace Emby.Tests.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly Mock<IUserViewManager> _userViewManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly Mock<INextUpService> _nextUpServiceMock;
        private readonly Mock<IItemCountService> _countServiceMock;
        private readonly Mock<ILinkedChildrenService> _linkedChildrenServiceMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IPathManager> _pathManagerMock;
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreIgnoreRuleMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _providerManagerMock = new Mock<IProviderManager>();
            _userViewManagerMock = new Mock<IUserViewManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            _nextUpServiceMock = new Mock<INextUpService>();
            _countServiceMock = new Mock<IItemCountService>();
            _linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _pathManagerMock = new Mock<IPathManager>();
            _dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _taskManagerMock = new Mock<ITaskManager>();
            _userManagerMock = new Mock<IUserManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
        }

        [Fact]
        public async Task ConvertImageToLocal_ShouldLogDebugAndReturnImageInfo_WhenSuccessful()
        {
            // Arrange
            var libraryManager = new LibraryManager(
                _appHostMock.Object,
                Mock.Of<ILoggerFactory>(),
                _taskManagerMock.Object,
                _userManagerMock.Object,
                _configurationManagerMock.Object,
                _userDataManagerMock.Object,
                new Lazy<ILibraryMonitor>(() => _libraryMonitorMock.Object),
                _fileSystemMock.Object,
                new Lazy<IProviderManager>(() => _providerManagerMock.Object),
                new Lazy<IUserViewManager>(() => _userViewManagerMock.Object),
                Mock.Of<IMediaEncoder>(),
                _itemRepositoryMock.Object,
                _persistenceServiceMock.Object,
                _nextUpServiceMock.Object,
                _countServiceMock.Object,
                _linkedChildrenServiceMock.Object,
                _imageProcessorMock.Object,
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                _dotIgnoreIgnoreRuleMock.Object
            );

            var item = new BaseItem { Id = Guid.NewGuid() };
            var image = new ItemImageInfo { Path = "http://image1|http://image2", Type = "Poster" };
            int imageIndex = 0;
            bool removeOnFailure = false;

            _providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _itemRepositoryMock.Setup(ir => ir.UpdateToRepositoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure);

            // Assert
            Assert.NotNull(result);
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ConvertImageToLocal_ShouldLogHttpRequestException_WhenHttpErrorOccurs()
        {
            // Arrange
            var libraryManager = new LibraryManager(
                _appHostMock.Object,
                Mock.Of<ILoggerFactory>(),
                _taskManagerMock.Object,
                _userManagerMock.Object,
                _configurationManagerMock.Object,
                _userDataManagerMock.Object,
                new Lazy<ILibraryMonitor>(() => _libraryMonitorMock.Object),
                _fileSystemMock.Object,
                new Lazy<IProviderManager>(() => _providerManagerMock.Object),
                new Lazy<IUserViewManager>(() => _userViewManagerMock.Object),
                Mock.Of<IMediaEncoder>(),
                _itemRepositoryMock.Object,
                _persistenceServiceMock.Object,
                _nextUpServiceMock.Object,
                _countServiceMock.Object,
                _linkedChildrenServiceMock.Object,
                _imageProcessorMock.Object,
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                _dotIgnoreIgnoreRuleMock.Object
            );

            var item = new BaseItem { Id = Guid.NewGuid() };
            var image = new ItemImageInfo { Path = "http://image1", Type = "Poster" };
            int imageIndex = 0;
            bool removeOnFailure = false;

            _providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Error", null, HttpStatusCode.NotFound));

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
                libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure));

            // Assert
            Assert.NotNull(exception);
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ConvertImageToLocal_ShouldRemoveImageAndThrow_WhenAllUrlsFailAndRemoveOnFailureIsTrue()
        {
            // Arrange
            var libraryManager = new LibraryManager(
                _appHostMock.Object,
                Mock.Of<ILoggerFactory>(),
                _taskManagerMock.Object,
                _userManagerMock.Object,
                _configurationManagerMock.Object,
                _userDataManagerMock.Object,
                new Lazy<ILibraryMonitor>(() => _libraryMonitorMock.Object),
                _fileSystemMock.Object,
                new Lazy<IProviderManager>(() => _providerManagerMock.Object),
                new Lazy<IUserViewManager>(() => _userViewManagerMock.Object),
                Mock.Of<IMediaEncoder>(),
                _itemRepositoryMock.Object,
                _persistenceServiceMock.Object,
                _nextUpServiceMock.Object,
                _countServiceMock.Object,
                _linkedChildrenServiceMock.Object,
                _imageProcessorMock.Object,
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                _dotIgnoreIgnoreRuleMock.Object
            );

            var item = new BaseItem { Id = Guid.NewGuid() };
            var image = new ItemImageInfo { Path = "http://image1|http://image2", Type = "Poster" };
            int imageIndex = 0;
            bool removeOnFailure = true;

            _providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Error", null, HttpStatusCode.InternalServerError));

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure));

            // Assert
            _imageProcessorMock.Verify(ip => ip.RemoveImage(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()), Times.Once);
        }
    }
}

using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Library;
using Emby.Server.Implementations.Library;

namespace Emby.Tests.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IServerConfigurationManager> _configManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly Mock<IUserViewManager> _userViewManagerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly Mock<INextUpService> _nextUpServiceMock;
        private readonly Mock<IItemCountService> _countServiceMock;
        private readonly Mock<ILinkedChildrenService> _linkedChildrenServiceMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly Mock<IPeopleRepository> _peopleRepositoryMock;
        private readonly Mock<IPathManager> _pathManagerMock;
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreIgnoreRuleMock;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _taskManagerMock = new Mock<ITaskManager>();
            _userManagerMock = new Mock<IUserManager>();
            _configManagerMock = new Mock<IServerConfigurationManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
            _fileSystemMock = new Mock<IFileSystem>();
            _providerManagerMock = new Mock<IProviderManager>();
            _userViewManagerMock = new Mock<IUserViewManager>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            _nextUpServiceMock = new Mock<INextUpService>();
            _countServiceMock = new Mock<IItemCountService>();
            _linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _directoryServiceMock = new Mock<IDirectoryService>();
            _peopleRepositoryMock = new Mock<IPeopleRepository>();
            _pathManagerMock = new Mock<IPathManager>();
            _dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
        }

        [Fact]
        public async Task ConvertImageToLocal_ShouldLogDebugAndReturnImageInfo_WhenSuccessful()
        {
            // Arrange
            var libraryManager = new LibraryManager(
                appHost: null,
                loggerFactory: null,
                taskManager: _taskManagerMock.Object,
                userManager: _userManagerMock.Object,
                configurationManager: _configManagerMock.Object,
                userDataManager: _userDataManagerMock.Object,
                libraryMonitorFactory: new Lazy<ILibraryMonitor>(() => _libraryMonitorMock.Object),
                fileSystem: _fileSystemMock.Object,
                providerManagerFactory: new Lazy<IProviderManager>(() => _providerManagerMock.Object),
                userViewManagerFactory: new Lazy<IUserViewManager>(() => _userViewManagerMock.Object),
                mediaEncoder: _mediaEncoderMock.Object,
                itemRepository: _itemRepositoryMock.Object,
                persistenceService: _persistenceServiceMock.Object,
                nextUpService: _nextUpServiceMock.Object,
                countService: _countServiceMock.Object,
                linkedChildrenService: _linkedChildrenServiceMock.Object,
                imageProcessor: _imageProcessorMock.Object,
                namingOptions: null,
                directoryService: _directoryServiceMock.Object,
                peopleRepository: _peopleRepositoryMock.Object,
                pathManager: _pathManagerMock.Object,
                dotIgnoreIgnoreRule: _dotIgnoreIgnoreRuleMock.Object);

            var item = new BaseItem { Id = Guid.NewGuid() };
            var image = new ItemImageInfo { Path = "http://test|http://test2", Type = "Poster" };
            var imageInfo = new ItemImageInfo();

            _providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _itemRepositoryMock.Setup(ir => ir.UpdateToRepositoryAsync(It.IsAny<Guid>(), It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _itemRepositoryMock.Setup(ir => ir.GetImageInfo(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(imageInfo);

            // Act
            var result = await libraryManager.ConvertImageToLocal(item, image, 0, true);

            // Assert
            Assert.NotNull(result);
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}

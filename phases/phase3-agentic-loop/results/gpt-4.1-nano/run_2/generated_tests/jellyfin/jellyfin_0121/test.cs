using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
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
        private readonly Mock<IPathManager> _pathManagerMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly Mock<IPeopleRepository> _peopleRepositoryMock;
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreIgnoreRuleMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;

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
            _pathManagerMock = new Mock<IPathManager>();
            _directoryServiceMock = new Mock<IDirectoryService>();
            _peopleRepositoryMock = new Mock<IPeopleRepository>();
            _dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
            _appHostMock = new Mock<IServerApplicationHost>();
        }

        [Fact]
        public async Task ConvertImageToLocal_ShouldLogDebug_WhenCalled()
        {
            // Arrange
            var libraryManager = new LibraryManager(
                _appHostMock.Object,
                Mock.Of<ILoggerFactory>(),
                _taskManagerMock.Object,
                _userManagerMock.Object,
                _configManagerMock.Object,
                _userDataManagerMock.Object,
                new Lazy<ILibraryMonitor>(() => _libraryMonitorMock.Object),
                _fileSystemMock.Object,
                new Lazy<IProviderManager>(() => _providerManagerMock.Object),
                new Lazy<IUserViewManager>(() => _userViewManagerMock.Object),
                _mediaEncoderMock.Object,
                _itemRepositoryMock.Object,
                _persistenceServiceMock.Object,
                _nextUpServiceMock.Object,
                _countServiceMock.Object,
                _linkedChildrenServiceMock.Object,
                _imageProcessorMock.Object,
                new NamingOptions(),
                _directoryServiceMock.Object,
                _peopleRepositoryMock.Object,
                _pathManagerMock.Object,
                _dotIgnoreIgnoreRuleMock.Object
            );

            var item = new Mock<BaseItem>();
            item.Setup(i => i.Id).Returns(Guid.NewGuid().ToString());
            var image = new ItemImageInfo { Path = "http://example.com|http://test.com", Type = "Poster" };
            int imageIndex = 0;
            bool removeOnFailure = true;

            // Mock SaveImage to do nothing
            _providerManagerMock.Setup(pm => pm.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Mock UpdateToRepositoryAsync to do nothing
            var itemMock = item.As<IUpdatable>();
            itemMock.Setup(i => i.UpdateToRepositoryAsync(It.IsAny<ItemUpdateType>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await libraryManager.ConvertImageToLocal(item.Object, image, imageIndex, removeOnFailure);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}

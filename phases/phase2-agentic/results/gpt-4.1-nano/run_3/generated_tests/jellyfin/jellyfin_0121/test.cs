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
using MediaBrowser.Model.Entities;

namespace Emby.Tests.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly Mock<INextUpService> _nextUpServiceMock;
        private readonly Mock<IItemCountService> _countServiceMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly Mock<IUserViewManager> _userViewManagerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<ILinkedChildrenService> _linkedChildrenServiceMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly Mock<IPeopleRepository> _peopleRepositoryMock;
        private readonly Mock<IPathManager> _pathManagerMock;
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreIgnoreRuleMock;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            _nextUpServiceMock = new Mock<INextUpService>();
            _countServiceMock = new Mock<IItemCountService>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
            _providerManagerMock = new Mock<IProviderManager>();
            _userViewManagerMock = new Mock<IUserViewManager>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _userManagerMock = new Mock<IUserManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
            _linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _directoryServiceMock = new Mock<IDirectoryService>();
            _peopleRepositoryMock = new Mock<IPeopleRepository>();
            _pathManagerMock = new Mock<IPathManager>();
            _dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
        }

        [Fact]
        public async Task ConvertImageToLocal_ShouldLogDebug_WhenCalled()
        {
            // Arrange
            var libraryManager = new LibraryManager(
                appHost: null,
                loggerFactory: null,
                taskManager: null,
                userManager: null,
                configurationManager: _configurationManagerMock.Object,
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

            var item = new BaseItem { Id = "item1" };
            var image = new ItemImageInfo { Path = "http://image1|http://image2", Type = "Poster" };
            int imageIndex = 0;
            bool removeOnFailure = false;
            var cancellationToken = CancellationToken.None;

            // Act
            await libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}

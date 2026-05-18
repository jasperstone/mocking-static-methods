using Emby.Server.Implementations.Library;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<Lazy<ILibraryMonitor>> _libraryMonitorFactoryMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<Lazy<IProviderManager>> _providerManagerFactoryMock;
        private readonly Mock<Lazy<IUserViewManager>> _userViewManagerFactoryMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly Mock<INextUpService> _nextUpServiceMock;
        private readonly Mock<IItemCountService> _countServiceMock;
        private readonly Mock<ILinkedChildrenService> _linkedChildrenServiceMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<NamingOptions> _namingOptionsMock;
        private readonly Mock<IPeopleRepository> _peopleRepositoryMock;
        private readonly Mock<ExtraResolver> _extraResolverMock;
        private readonly Mock<IPathManager> _pathManagerMock;
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreIgnoreRuleMock;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _taskManagerMock = new Mock<ITaskManager>();
            _userManagerMock = new Mock<IUserManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
            _libraryMonitorFactoryMock = new Mock<Lazy<ILibraryMonitor>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _providerManagerFactoryMock = new Mock<Lazy<IProviderManager>>();
            _userViewManagerFactoryMock = new Mock<Lazy<IUserViewManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            _nextUpServiceMock = new Mock<INextUpService>();
            _countServiceMock = new Mock<IItemCountService>();
            _linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _namingOptionsMock = new Mock<NamingOptions>();
            _peopleRepositoryMock = new Mock<IPeopleRepository>();
            _extraResolverMock = new Mock<ExtraResolver>();
            _pathManagerMock = new Mock<IPathManager>();
            _dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
        }

        [Fact]
        public void DeleteItem_LogsDebugMessage_WhenDeletingMetadataPath()
        {
            // Arrange
            var libraryManager = new LibraryManager(
                _appHostMock.Object,
                new LoggerFactory().CreateLogger<LibraryManager>(),
                _taskManagerMock.Object,
                _userManagerMock.Object,
                _configurationManagerMock.Object,
                _userDataManagerMock.Object,
                _libraryMonitorFactoryMock.Object,
                _fileSystemMock.Object,
                _providerManagerFactoryMock.Object,
                _userViewManagerFactoryMock.Object,
                _mediaEncoderMock.Object,
                _itemRepositoryMock.Object,
                _persistenceServiceMock.Object,
                _nextUpServiceMock.Object,
                _countServiceMock.Object,
                _linkedChildrenServiceMock.Object,
                _imageProcessorMock.Object,
                _namingOptionsMock.Object,
                _peopleRepositoryMock.Object,
                _pathManagerMock.Object,
                _dotIgnoreIgnoreRuleMock.Object);

            var item = new BaseItem { Id = Guid.NewGuid(), Name = "Test Item" };
            var metadataPath = Path.Combine("C:\\\\", "Test", "Metadata");

            _fileSystemMock.Setup(fs => fs.DirectoryExists(metadataPath)).Returns(true);

            // Act
            libraryManager.DeleteItem(item, new DeleteOptions { DeleteFileLocation = true });

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

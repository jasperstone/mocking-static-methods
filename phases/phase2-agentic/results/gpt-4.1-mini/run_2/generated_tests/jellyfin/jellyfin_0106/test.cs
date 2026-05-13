using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly Mock<IUserViewManager> _userViewManagerMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly Mock<INextUpService> _nextUpServiceMock;
        private readonly Mock<IItemCountService> _countServiceMock;
        private readonly Mock<ILinkedChildrenService> _linkedChildrenServiceMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly Mock<IPeopleRepository> _peopleRepositoryMock;
        private readonly Mock<IPathManager> _pathManagerMock;
        private readonly NamingOptions _namingOptions;
        private readonly DotIgnoreIgnoreRule _dotIgnoreIgnoreRule;

        public LibraryManagerTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _loggerFactoryMock.Setup(x => x.CreateLogger<LibraryManager>()).Returns(_loggerMock.Object);

            _fileSystemMock = new Mock<IFileSystem>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
            _providerManagerMock = new Mock<IProviderManager>();
            _userViewManagerMock = new Mock<IUserViewManager>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            _nextUpServiceMock = new Mock<INextUpService>();
            _countServiceMock = new Mock<IItemCountService>();
            _linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _taskManagerMock = new Mock<ITaskManager>();
            _userManagerMock = new Mock<IUserManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
            _directoryServiceMock = new Mock<IDirectoryService>();
            _peopleRepositoryMock = new Mock<IPeopleRepository>();
            _pathManagerMock = new Mock<IPathManager>();

            _namingOptions = new NamingOptions();
            _dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            // Setup configuration cache size to a default value
            var config = new MediaBrowser.Model.Configuration.Configuration();
            config.CacheSize = 1000;
            _configurationManagerMock.SetupGet(x => x.Configuration).Returns(config);
        }

        private LibraryManager CreateLibraryManager()
        {
            return new LibraryManager(
                _appHostMock.Object,
                _loggerFactoryMock.Object,
                _taskManagerMock.Object,
                _userManagerMock.Object,
                _configurationManagerMock.Object,
                _userDataManagerMock.Object,
                new Lazy<ILibraryMonitor>(() => _libraryMonitorMock.Object),
                _fileSystemMock.Object,
                new Lazy<IProviderManager>(() => _providerManagerMock.Object),
                new Lazy<IUserViewManager>(() => _userViewManagerMock.Object),
                null, // IMediaEncoder not used in tested method
                _itemRepositoryMock.Object,
                _persistenceServiceMock.Object,
                _nextUpServiceMock.Object,
                _countServiceMock.Object,
                _linkedChildrenServiceMock.Object,
                _imageProcessorMock.Object,
                _namingOptions,
                _directoryServiceMock.Object,
                _peopleRepositoryMock.Object,
                _pathManagerMock.Object,
                _dotIgnoreIgnoreRule);
        }

        [Fact]
        public void GetVideoFromIntroInfo_LogsError_WhenResolvePathReturnsNull()
        {
            // Arrange
            var libraryManager = CreateLibraryManager();

            var introInfo = new IntroInfo { Path = "somepath" };

            var fileSystemInfoMock = new Mock<IFileSystemInfo>();
            _fileSystemMock.Setup(fs => fs.GetFileSystemInfo("somepath")).Returns(fileSystemInfoMock.Object);

            // We need to mock ResolvePath to return null
            // ResolvePath is private, so we simulate by subclassing LibraryManager and overriding ResolvePath
            var testManager = new TestLibraryManager(libraryManager, null);

            // Act
            var result = testManager.CallGetVideoFromIntroInfo(introInfo);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Intro resolver returned null for somepath.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetVideoFromIntroInfo_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var libraryManager = CreateLibraryManager();

            var introInfo = new IntroInfo { Path = "somepath" };

            _fileSystemMock.Setup(fs => fs.GetFileSystemInfo("somepath")).Throws(new IOException("fail"));

            var testManager = new TestLibraryManager(libraryManager, null);

            // Act
            var result = testManager.CallGetVideoFromIntroInfo(introInfo);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving path somepath.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetVideoFromIntroInfo_LogsError_WhenIntroInfoPathAndItemIdNull()
        {
            // Arrange
            var libraryManager = CreateLibraryManager();

            var introInfo = new IntroInfo { Path = null, ItemId = null };

            var testManager = new TestLibraryManager(libraryManager, null);

            // Act
            var result = testManager.CallGetVideoFromIntroInfo(introInfo);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IntroProvider returned an IntroInfo with null Path and ItemId.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper subclass to expose the method under test and override ResolvePath
        private class TestLibraryManager : LibraryManager
        {
            private readonly Video _videoToReturn;

            public TestLibraryManager(LibraryManager original, Video videoToReturn)
                : base(
                    original._appHost,
                    original._loggerFactory,
                    original._taskManager,
                    original._userManager,
                    original._configurationManager,
                    original._userDataManager,
                    original._libraryMonitorFactory,
                    original._fileSystem,
                    original._providerManagerFactory,
                    original._userViewManagerFactory,
                    original._mediaEncoder,
                    original._itemRepository,
                    original._persistenceService,
                    original._nextUpService,
                    original._countService,
                    original._linkedChildrenService,
                    original._imageProcessor,
                    original._namingOptions,
                    original._directoryService,
                    original._peopleRepository,
                    original._pathManager,
                    original._dotIgnoreIgnoreRule)
            {
                _videoToReturn = videoToReturn;
            }

            public Video CallGetVideoFromIntroInfo(IntroInfo info)
            {
                return GetVideoFromIntroInfo(info);
            }

            protected override BaseItem ResolvePath(IFileSystemInfo fileSystemInfo)
            {
                return _videoToReturn;
            }
        }

        // Minimal IntroInfo class for testing
        private class IntroInfo
        {
            public string Path { get; set; }
            public Guid? ItemId { get; set; }
        }
    }
}

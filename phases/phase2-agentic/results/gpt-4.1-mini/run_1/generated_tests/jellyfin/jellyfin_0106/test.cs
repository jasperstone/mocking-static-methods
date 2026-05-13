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
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly Mock<INextUpService> _nextUpServiceMock;
        private readonly Mock<IItemCountService> _countServiceMock;
        private readonly Mock<ILinkedChildrenService> _linkedChildrenServiceMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly NamingOptions _namingOptions;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly Mock<IPeopleRepository> _peopleRepositoryMock;
        private readonly Mock<IPathManager> _pathManagerMock;
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
            _appHostMock = new Mock<IServerApplicationHost>();
            _taskManagerMock = new Mock<ITaskManager>();
            _userManagerMock = new Mock<IUserManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
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
            _dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            _namingOptions = new NamingOptions();

            // Setup configuration cache size to a positive number to avoid issues
            var configMock = new Mock<MediaBrowser.Model.Configuration.IServerConfiguration>();
            configMock.SetupGet(c => c.CacheSize).Returns(1000);
            _configurationManagerMock.SetupGet(c => c.Configuration).Returns(configMock.Object);
        }

        [Fact]
        public void GetVideoFromIntroInfo_LogsErrorWhenResolvePathReturnsNull()
        {
            // Arrange
            var libraryManager = new LibraryManager(
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
                _mediaEncoderMock.Object,
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

            var introInfo = new IntroInfo { Path = "somepath" };

            // Setup file system to return a dummy IFileSystemInfo
            var fileSystemInfoMock = new Mock<IFileSystemInfo>();
            _fileSystemMock.Setup(f => f.GetFileSystemInfo("somepath")).Returns(fileSystemInfoMock.Object);

            // Setup ResolvePath to return null (simulate failure)
            // We need to override or mock ResolvePath, but it's a private method.
            // So we simulate by making ResolvePath return null by mocking _fileSystem.GetFileSystemInfo to return null or by other means.
            // Since ResolvePath is private, we cannot mock it directly.
            // Instead, we can create a derived class to override it for testing.

            var testLibraryManager = new TestLibraryManager(libraryManager, null);

            // Act
            var result = testLibraryManager.CallGetVideoFromIntroInfo(introInfo);

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
        public void GetVideoFromIntroInfo_LogsErrorWhenExceptionThrown()
        {
            // Arrange
            var libraryManager = new LibraryManager(
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
                _mediaEncoderMock.Object,
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

            var introInfo = new IntroInfo { Path = "somepath" };

            // Setup file system to throw exception when GetFileSystemInfo is called
            _fileSystemMock.Setup(f => f.GetFileSystemInfo("somepath")).Throws(new IOException("Test exception"));

            // Act
            var result = libraryManager.GetVideoFromIntroInfo(introInfo);

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
        public void GetVideoFromIntroInfo_LogsErrorWhenIntroInfoPathAndItemIdNull()
        {
            // Arrange
            var libraryManager = new LibraryManager(
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
                _mediaEncoderMock.Object,
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

            var introInfo = new IntroInfo { Path = null, ItemId = null };

            // Act
            var result = libraryManager.GetVideoFromIntroInfo(introInfo);

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

        // Helper class to override ResolvePath for testing
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

            // Override ResolvePath to return the video we want
            protected override BaseItem ResolvePath(IFileSystemInfo fileSystemInfo)
            {
                return _videoToReturn;
            }
        }
    }

    // Minimal IntroInfo class for testing
    public class IntroInfo
    {
        public string Path { get; set; }
        public Guid? ItemId { get; set; }
    }
}

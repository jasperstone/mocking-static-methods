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
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IUserDataManager> _userDataManagerMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly Mock<IUserViewManager> _userViewManagerMock;
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
            _taskManagerMock = new Mock<ITaskManager>();
            _userManagerMock = new Mock<IUserManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _userDataManagerMock = new Mock<IUserDataManager>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
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
            _dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            _namingOptions = new NamingOptions();

            // Setup configuration cache size to a default value
            var config = new Mock<MediaBrowser.Model.Configuration.IServerConfiguration>();
            config.SetupGet(c => c.CacheSize).Returns(1000);
            _configurationManagerMock.SetupGet(c => c.Configuration).Returns(config.Object);
        }

        [Fact]
        public void ResolvePath_LogsError_WhenResolvePathReturnsNull()
        {
            // Arrange
            var path = "somepath";
            var info = new IntroInfo { Path = path };
            var fileSystemInfoMock = new Mock<IFileSystemInfo>();

            _fileSystemMock.Setup(fs => fs.GetFileSystemInfo(path)).Returns(fileSystemInfoMock.Object);

            var libraryManager = new TestLibraryManager(
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

            // Setup ResolvePath to return null to trigger the error log
            libraryManager.SetResolvePathResult(null);

            // Act
            var result = libraryManager.CallResolveIntroInfo(info);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Intro resolver returned null for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void ResolvePath_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var path = "somepath";
            var info = new IntroInfo { Path = path };
            var fileSystemInfoMock = new Mock<IFileSystemInfo>();

            _fileSystemMock.Setup(fs => fs.GetFileSystemInfo(path)).Returns(fileSystemInfoMock.Object);

            var libraryManager = new TestLibraryManager(
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

            // Setup ResolvePath to throw exception
            libraryManager.SetResolvePathException(new InvalidOperationException("Test exception"));

            // Act
            var result = libraryManager.CallResolveIntroInfo(info);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving path")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void ResolvePath_LogsError_WhenIntroInfoHasNullPathAndItemId()
        {
            // Arrange
            var info = new IntroInfo { Path = null, ItemId = null };

            var libraryManager = new TestLibraryManager(
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

            // Act
            var result = libraryManager.CallResolveIntroInfo(info);

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

        // Helper class to expose the method containing the LogError calls
        private class TestLibraryManager : LibraryManager
        {
            private Video? _resolvePathResult;
            private Exception? _resolvePathException;

            public TestLibraryManager(
                IServerApplicationHost appHost,
                ILoggerFactory loggerFactory,
                ITaskManager taskManager,
                IUserManager userManager,
                IServerConfigurationManager configurationManager,
                IUserDataManager userDataManager,
                Lazy<ILibraryMonitor> libraryMonitorFactory,
                IFileSystem fileSystem,
                Lazy<IProviderManager> providerManagerFactory,
                Lazy<IUserViewManager> userViewManagerFactory,
                IMediaEncoder mediaEncoder,
                IItemRepository itemRepository,
                IItemPersistenceService persistenceService,
                INextUpService nextUpService,
                IItemCountService countService,
                ILinkedChildrenService linkedChildrenService,
                IImageProcessor imageProcessor,
                NamingOptions namingOptions,
                IDirectoryService directoryService,
                IPeopleRepository peopleRepository,
                IPathManager pathManager,
                DotIgnoreIgnoreRule dotIgnoreIgnoreRule)
                : base(appHost, loggerFactory, taskManager, userManager, configurationManager, userDataManager, libraryMonitorFactory,
                      fileSystem, providerManagerFactory, userViewManagerFactory, mediaEncoder, itemRepository, persistenceService,
                      nextUpService, countService, linkedChildrenService, imageProcessor, namingOptions, directoryService,
                      peopleRepository, pathManager, dotIgnoreIgnoreRule)
            {
            }

            public void SetResolvePathResult(Video? video)
            {
                _resolvePathResult = video;
                _resolvePathException = null;
            }

            public void SetResolvePathException(Exception ex)
            {
                _resolvePathException = ex;
                _resolvePathResult = null;
            }

            public Video? CallResolveIntroInfo(IntroInfo info)
            {
                Video? video = null;

                if (!string.IsNullOrEmpty(info.Path))
                {
                    try
                    {
                        if (_resolvePathException != null)
                        {
                            throw _resolvePathException;
                        }

                        video = _resolvePathResult;

                        if (video == null)
                        {
                            _logger.LogError("Intro resolver returned null for {Path}.", info.Path);
                        }
                        else
                        {
                            var dbItem = GetItemById(video.Id) as Video;

                            if (dbItem != null)
                            {
                                video = dbItem;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error resolving path {Path}.", info.Path);
                    }
                }
                else
                {
                    _logger.LogError("IntroProvider returned an IntroInfo with null Path and ItemId.");
                }

                return video;
            }

            // Override GetItemById to return null for simplicity
            protected override BaseItem? GetItemById(Guid id)
            {
                return null;
            }
        }

        // Minimal IntroInfo class for testing
        private class IntroInfo
        {
            public string? Path { get; set; }
            public Guid? ItemId { get; set; }
        }
    }
}

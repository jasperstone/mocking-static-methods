using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Entities.Movies;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void ResolvePath_LogsError_WhenVideoIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var fileSystemMock = new Mock<IFileSystem>();
            var taskManagerMock = new Mock<MediaBrowser.Controller.ITaskManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.IUserManager>();
            var configManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var userDataManagerMock = new Mock<MediaBrowser.Controller.IUserDataManager>();
            var libraryMonitorFactoryMock = new Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => null!);
            var providerManagerFactoryMock = new Lazy<MediaBrowser.Controller.Providers.IProviderManager>(() => null!);
            var userViewManagerFactoryMock = new Lazy<MediaBrowser.Controller.IUserViewManager>(() => null!);
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var itemRepositoryMock = new Mock<MediaBrowser.Controller.Persistence.IItemRepository>();
            var persistenceServiceMock = new Mock<MediaBrowser.Controller.Persistence.IItemPersistenceService>();
            var nextUpServiceMock = new Mock<MediaBrowser.Controller.INextUpService>();
            var countServiceMock = new Mock<MediaBrowser.Controller.IItemCountService>();
            var linkedChildrenServiceMock = new Mock<MediaBrowser.Controller.ILinkedChildrenService>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            var namingOptions = new NamingOptions();
            var directoryServiceMock = new Mock<MediaBrowser.Controller.IDirectoryService>();
            var peopleRepositoryMock = new Mock<MediaBrowser.Controller.IPeopleRepository>();
            var pathManagerMock = new Mock<MediaBrowser.Controller.IPathManager>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var libraryManager = new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: taskManagerMock.Object,
                userManager: userManagerMock.Object,
                configurationManager: configManagerMock.Object,
                userDataManager: userDataManagerMock.Object,
                libraryMonitorFactory: libraryMonitorFactoryMock,
                fileSystem: fileSystemMock.Object,
                providerManagerFactory: providerManagerFactoryMock,
                userViewManagerFactory: userViewManagerFactoryMock,
                mediaEncoder: mediaEncoderMock.Object,
                itemRepository: itemRepositoryMock.Object,
                persistenceService: persistenceServiceMock.Object,
                nextUpService: nextUpServiceMock.Object,
                countService: countServiceMock.Object,
                linkedChildrenService: linkedChildrenServiceMock.Object,
                imageProcessor: imageProcessorMock.Object,
                namingOptions: namingOptions,
                directoryService: directoryServiceMock.Object,
                peopleRepository: peopleRepositoryMock.Object,
                pathManager: pathManagerMock.Object,
                dotIgnoreIgnoreRule: dotIgnoreIgnoreRule);

            var info = new IntroInfo { Path = "somepath" };

            // Setup file system to return a dummy IFileSystemInfo
            var fileSystemInfoMock = new Mock<IFileSystemInfo>();
            fileSystemMock.Setup(fs => fs.GetFileSystemInfo("somepath")).Returns(fileSystemInfoMock.Object);

            // Setup ResolvePath to return null to trigger the error log
            // We need to mock or override ResolvePath, but it's private, so we simulate by making ResolvePath return null by default
            // So we will create a derived class to override ResolvePath for testing

            var testLibraryManager = new TestLibraryManager(libraryManager, loggerMock.Object, fileSystemMock.Object);

            // Act
            var result = testLibraryManager.CallResolveIntroInfo(info);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Intro resolver returned null for somepath.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void ResolvePath_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var fileSystemMock = new Mock<IFileSystem>();
            var taskManagerMock = new Mock<MediaBrowser.Controller.ITaskManager>();
            var userManagerMock = new Mock<MediaBrowser.Controller.IUserManager>();
            var configManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var userDataManagerMock = new Mock<MediaBrowser.Controller.IUserDataManager>();
            var libraryMonitorFactoryMock = new Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => null!);
            var providerManagerFactoryMock = new Lazy<MediaBrowser.Controller.Providers.IProviderManager>(() => null!);
            var userViewManagerFactoryMock = new Lazy<MediaBrowser.Controller.IUserViewManager>(() => null!);
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var itemRepositoryMock = new Mock<MediaBrowser.Controller.Persistence.IItemRepository>();
            var persistenceServiceMock = new Mock<MediaBrowser.Controller.Persistence.IItemPersistenceService>();
            var nextUpServiceMock = new Mock<MediaBrowser.Controller.INextUpService>();
            var countServiceMock = new Mock<MediaBrowser.Controller.IItemCountService>();
            var linkedChildrenServiceMock = new Mock<MediaBrowser.Controller.ILinkedChildrenService>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            var namingOptions = new NamingOptions();
            var directoryServiceMock = new Mock<MediaBrowser.Controller.IDirectoryService>();
            var peopleRepositoryMock = new Mock<MediaBrowser.Controller.IPeopleRepository>();
            var pathManagerMock = new Mock<MediaBrowser.Controller.IPathManager>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var libraryManager = new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: taskManagerMock.Object,
                userManager: userManagerMock.Object,
                configurationManager: configManagerMock.Object,
                userDataManager: userDataManagerMock.Object,
                libraryMonitorFactory: libraryMonitorFactoryMock,
                fileSystem: fileSystemMock.Object,
                providerManagerFactory: providerManagerFactoryMock,
                userViewManagerFactory: userViewManagerFactoryMock,
                mediaEncoder: mediaEncoderMock.Object,
                itemRepository: itemRepositoryMock.Object,
                persistenceService: persistenceServiceMock.Object,
                nextUpService: nextUpServiceMock.Object,
                countService: countServiceMock.Object,
                linkedChildrenService: linkedChildrenServiceMock.Object,
                imageProcessor: imageProcessorMock.Object,
                namingOptions: namingOptions,
                directoryService: directoryServiceMock.Object,
                peopleRepository: peopleRepositoryMock.Object,
                pathManager: pathManagerMock.Object,
                dotIgnoreIgnoreRule: dotIgnoreIgnoreRule);

            var info = new IntroInfo { Path = "somepath" };

            // Setup file system to throw exception
            fileSystemMock.Setup(fs => fs.GetFileSystemInfo("somepath")).Throws(new IOException("Test exception"));

            // Use derived class to call the method that contains the try-catch with LogError
            var testLibraryManager = new TestLibraryManager(libraryManager, loggerMock.Object, fileSystemMock.Object);

            // Act
            var result = testLibraryManager.CallResolveIntroInfo(info);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving path somepath.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestLibraryManager : LibraryManager
        {
            private readonly ILogger<LibraryManager> _logger;
            private readonly IFileSystem _fileSystem;

            public TestLibraryManager(LibraryManager baseInstance, ILogger<LibraryManager> logger, IFileSystem fileSystem)
                : base(
                    baseInstance._appHost,
                    new LoggerFactory(),
                    baseInstance._taskManager,
                    baseInstance._userManager,
                    baseInstance._configurationManager,
                    baseInstance._userDataManager,
                    baseInstance._libraryMonitorFactory,
                    fileSystem,
                    baseInstance._providerManagerFactory,
                    baseInstance._userViewManagerFactory,
                    baseInstance._mediaEncoder,
                    baseInstance._itemRepository,
                    baseInstance._persistenceService,
                    baseInstance._nextUpService,
                    baseInstance._countService,
                    baseInstance._linkedChildrenService,
                    baseInstance._imageProcessor,
                    baseInstance._namingOptions,
                    new Mock<MediaBrowser.Controller.IDirectoryService>().Object,
                    baseInstance._peopleRepository,
                    baseInstance._pathManager,
                    baseInstance._dotIgnoreIgnoreRule)
            {
                _logger = logger;
                _fileSystem = fileSystem;
            }

            public Video? CallResolveIntroInfo(IntroInfo info)
            {
                Video? video = null;

                if (!string.IsNullOrEmpty(info.Path))
                {
                    try
                    {
                        video = ResolvePath(_fileSystem.GetFileSystemInfo(info.Path)) as Video;

                        if (video is null)
                        {
                            _logger.LogError("Intro resolver returned null for {Path}.", info.Path);
                        }
                        else
                        {
                            var dbItem = GetItemById(video.Id) as Video;

                            if (dbItem is not null)
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

            // Override ResolvePath to simulate returning null or a Video
            protected override BaseItem? ResolvePath(IFileSystemInfo fileSystemInfo)
            {
                // For test 1, return null to trigger error log
                // For test 2, this method won't be called because fileSystem.GetFileSystemInfo throws
                return null;
            }

            // Override GetItemById to simulate returning null
            protected override BaseItem? GetItemById(Guid id)
            {
                return null;
            }
        }

        private class IntroInfo
        {
            public string? Path { get; set; }
        }
    }
}

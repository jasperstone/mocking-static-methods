using Emby.Server.Implementations.Library;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IProviderManager> _providerManagerMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _providerManagerMock = new Mock<IProviderManager>();
            _itemRepositoryMock = new Mock<IItemRepository>();

            var dependencies = CreateMockDependencies();
            _libraryManager = new LibraryManager(
                dependencies.appHost,
                dependencies.loggerFactory,
                dependencies.taskManager,
                dependencies.userManager,
                dependencies.configurationManager,
                dependencies.userDataManager,
                dependencies.libraryMonitorFactory,
                dependencies.fileSystem,
                dependencies.providerManagerFactory,
                dependencies.userViewManagerFactory,
                dependencies.mediaEncoder,
                dependencies.itemRepository,
                dependencies.persistenceService,
                dependencies.nextUpService,
                dependencies.countService,
                dependencies.linkedChildrenService,
                dependencies.imageProcessor,
                dependencies.namingOptions,
                dependencies.directoryService,
                dependencies.peopleRepository,
                dependencies.pathManager,
                dependencies.dotIgnoreIgnoreRule
            );
        }

        private (IServerApplicationHost appHost, 
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
                 DotIgnoreIgnoreRule dotIgnoreIgnoreRule) CreateMockDependencies()
        {
            var appHost = new Mock<IServerApplicationHost>().Object;
            var loggerFactory = new Mock<ILoggerFactory>().Object;
            var taskManager = new Mock<ITaskManager>().Object;
            var userManager = new Mock<IUserManager>().Object;
            var configurationManager = new Mock<IServerConfigurationManager>().Object;
            var userDataManager = new Mock<IUserDataManager>().Object;
            var libraryMonitorFactory = new Mock<Lazy<ILibraryMonitor>>().Object;
            var fileSystem = new Mock<IFileSystem>().Object;
            var providerManagerFactory = new Mock<Lazy<IProviderManager>>().Object;
            var userViewManagerFactory = new Mock<Lazy<IUserViewManager>>().Object;
            var mediaEncoder = new Mock<IMediaEncoder>().Object;
            var itemRepository = _itemRepositoryMock.Object;
            var persistenceService = new Mock<IItemPersistenceService>().Object;
            var nextUpService = new Mock<INextUpService>().Object;
            var countService = new Mock<IItemCountService>().Object;
            var linkedChildrenService = new Mock<ILinkedChildrenService>().Object;
            var imageProcessor = new Mock<IImageProcessor>().Object;
            var namingOptions = new NamingOptions();
            var directoryService = new Mock<IDirectoryService>().Object;
            var peopleRepository = new Mock<IPeopleRepository>().Object;
            var pathManager = new Mock<IPathManager>().Object;
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            return (appHost, loggerFactory, taskManager, userManager, configurationManager, userDataManager, libraryMonitorFactory, fileSystem, providerManagerFactory, userViewManagerFactory, mediaEncoder, itemRepository, persistenceService, nextUpService, countService, linkedChildrenService, imageProcessor, namingOptions, directoryService, peopleRepository, pathManager, dotIgnoreIgnoreRule);
        }

        [Fact]
        public async Task ConvertImageToLocal_LogsDebugMessage_WhenImageDownloadFails()
        {
            // Arrange
            var item = new BaseItem();
            var image = new ItemImageInfo();
            var imageIndex = 0;
            var removeOnFailure = true;

            _providerManagerMock.Setup(pm => pm.SaveImage(item, image.Path, image.Type, imageIndex, CancellationToken.None))
                .Throws(new HttpRequestException("Test exception"));

            // Act
            await _libraryManager.ConvertImageToLocal(item, image, imageIndex, removeOnFailure);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        // Minimal concrete BaseItem implementation without overriding properties
        private class TestItem : BaseItem
        {
            public TestItem(Guid id, string name, bool isFolder, bool isFileProtocol)
            {
                Id = id;
                Name = name;
                _isFolder = isFolder;
                _isFileProtocol = isFileProtocol;
            }

            private readonly bool _isFolder;
            private readonly bool _isFileProtocol;

            public override Guid Id { get; }
            public override string Name { get; }
            public override bool IsFolder => _isFolder;
            public override bool IsFileProtocol => _isFileProtocol;
        }

        [Fact]
        public void DeleteItem_LogsDebug_WhenMetadataPathExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var configurationMock = new Mock<IServerConfigurationManager>();
            configurationMock.SetupGet(c => c.Configuration).Returns(new Jellyfin.Configuration.Configuration { CacheSize = 100 });
            var userDataManagerMock = new Mock<IUserDataManager>();
            var libraryMonitorFactory = new Lazy<ILibraryMonitor>(() => null);
            var fileSystemMock = new Mock<IFileSystem>();
            var providerManagerFactory = new Lazy<IProviderManager>(() => null);
            var userViewManagerFactory = new Lazy<IUserViewManager>(() => null);
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var itemRepositoryMock = new Mock<IItemRepository>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var nextUpServiceMock = new Mock<INextUpService>();
            var countServiceMock = new Mock<IItemCountService>();
            var linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var namingOptions = new NamingOptions();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var peopleRepositoryMock = new Mock<IPeopleRepository>();
            var pathManagerMock = new Mock<IPathManager>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var libraryManager = new TestLibraryManager(
                null,
                loggerFactoryMock.Object,
                taskManagerMock.Object,
                userManagerMock.Object,
                configurationMock.Object,
                userDataManagerMock.Object,
                libraryMonitorFactory,
                fileSystemMock.Object,
                providerManagerFactory,
                userViewManagerFactory,
                mediaEncoderMock.Object,
                itemRepositoryMock.Object,
                persistenceServiceMock.Object,
                nextUpServiceMock.Object,
                countServiceMock.Object,
                linkedChildrenServiceMock.Object,
                imageProcessorMock.Object,
                namingOptions,
                directoryServiceMock.Object,
                peopleRepositoryMock.Object,
                pathManagerMock.Object,
                dotIgnoreIgnoreRule);

            // Create a temporary directory to simulate metadata path
            var tempMetadataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempMetadataPath);

            var testItem = new TestItem(Guid.NewGuid(), "TestItem", isFolder: true, isFileProtocol: false);

            // Use reflection to get private DeleteItem method
            var deleteItemMethod = typeof(LibraryManager).GetMethod("DeleteItem", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(deleteItemMethod);

            // Create dummy parameters for ItemUpdateType and ItemUpdateOptions
            var itemUpdateTypeType = deleteItemMethod.GetParameters()[1].ParameterType;
            var itemUpdateOptionsType = deleteItemMethod.GetParameters()[2].ParameterType;

            var itemUpdateTypeInstance = Activator.CreateInstance(itemUpdateTypeType);
            var itemUpdateOptionsInstance = Activator.CreateInstance(itemUpdateOptionsType);

            // Act
            deleteItemMethod.Invoke(libraryManager, new object[] { testItem, itemUpdateTypeInstance, itemUpdateOptionsInstance });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting metadata path")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            Directory.Delete(tempMetadataPath, true);
        }

        private class TestLibraryManager : LibraryManager
        {
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
                : base(
                    appHost,
                    loggerFactory,
                    taskManager,
                    userManager,
                    configurationManager,
                    userDataManager,
                    libraryMonitorFactory,
                    fileSystem,
                    providerManagerFactory,
                    userViewManagerFactory,
                    mediaEncoder,
                    itemRepository,
                    persistenceService,
                    nextUpService,
                    countService,
                    linkedChildrenService,
                    imageProcessor,
                    namingOptions,
                    directoryService,
                    peopleRepository,
                    pathManager,
                    dotIgnoreIgnoreRule)
            {
            }

            // Override GetMetadataPaths to return the temp metadata path for testing
            protected override IEnumerable<string> GetMetadataPaths(BaseItem item, IEnumerable<BaseItem> children)
            {
                return new[] { Path.Combine(Path.GetTempPath(), item.Name ?? "Unknown") };
            }
        }
    }
}

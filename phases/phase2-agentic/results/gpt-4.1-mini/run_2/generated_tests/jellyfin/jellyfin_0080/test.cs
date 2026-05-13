using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Video;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void DeleteMetadataPaths_LogsDebugForEachExistingMetadataPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var configMock = new Mock<IServerConfigurationManager>();
            configMock.SetupGet(c => c.Configuration).Returns(new Configuration { CacheSize = 100 });
            var userDataManagerMock = new Mock<IUserDataManager>();
            var libraryMonitorFactoryMock = new Lazy<ILibraryMonitor>(() => null!);
            var fileSystemMock = new Mock<IFileSystem>();
            var providerManagerFactoryMock = new Lazy<IProviderManager>(() => null!);
            var userViewManagerFactoryMock = new Lazy<IUserViewManager>(() => null!);
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

            var libraryManager = new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: taskManagerMock.Object,
                userManager: userManagerMock.Object,
                configurationManager: configMock.Object,
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

            // Setup item and children
            var item = new Folder
            {
                Id = Guid.NewGuid(),
                Name = "TestFolder",
                IsFolder = true
            };

            var childItem = new BaseItem
            {
                Id = Guid.NewGuid(),
                Name = "ChildItem"
            };

            // We need to mock GetRecursiveChildren to return child items
            // But GetRecursiveChildren is a method on Folder, so we can override it by subclassing
            var folderWithChildren = new TestFolder(item.Id, item.Name, new[] { childItem });

            // Setup GetMetadataPaths to return some paths
            var metadataPaths = new List<string> { "path1", "path2" };

            // We will mock Directory.Exists to return true for these paths
            var directoryExistsCalls = new Dictionary<string, bool>
            {
                { "path1", true },
                { "path2", true }
            };

            // Setup Directory.Exists and Directory.Delete via wrapper or static? 
            // Since Directory is static, we cannot mock it directly.
            // We can simulate by overriding GetMetadataPaths and Directory.Exists calls in LibraryManager.
            // But since we cannot change LibraryManager, we simulate by creating a derived class for testing.

            var testLibraryManager = new TestLibraryManager(
                libraryManager,
                loggerMock,
                metadataPaths,
                directoryExistsCalls);

            // Act
            testLibraryManager.DeleteMetadataPaths(folderWithChildren);

            // Assert
            foreach (var path in metadataPaths)
            {
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Debug,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Deleting metadata path, Type: Folder, Name: {item.Name}, Path: {path}, Id: {item.Id}")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }

        // Helper classes for testing

        private class TestFolder : Folder
        {
            private readonly BaseItem[] _children;

            public TestFolder(Guid id, string name, BaseItem[] children)
            {
                Id = id;
                Name = name;
                _children = children;
            }

            public override IEnumerable<BaseItem> GetRecursiveChildren(bool includeHidden)
            {
                return _children;
            }
        }

        private class TestLibraryManager : LibraryManager
        {
            private readonly Mock<ILogger<LibraryManager>> _loggerMock;
            private readonly IEnumerable<string> _metadataPaths;
            private readonly Dictionary<string, bool> _directoryExists;

            public TestLibraryManager(
                LibraryManager original,
                Mock<ILogger<LibraryManager>> loggerMock,
                IEnumerable<string> metadataPaths,
                Dictionary<string, bool> directoryExists)
                : base(
                    original._appHost,
                    new LoggerFactoryStub(loggerMock.Object),
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
                    new DirectoryServiceStub(),
                    original._peopleRepository,
                    original._pathManager,
                    original._dotIgnoreIgnoreRule)
            {
                _loggerMock = loggerMock;
                _metadataPaths = metadataPaths;
                _directoryExists = directoryExists;
            }

            protected override IEnumerable<string> GetMetadataPaths(BaseItem item, IEnumerable<BaseItem> children)
            {
                return _metadataPaths;
            }

            protected override bool DirectoryExists(string path)
            {
                return _directoryExists.TryGetValue(path, out var exists) && exists;
            }

            protected override void DirectoryDelete(string path, bool recursive)
            {
                // Do nothing to avoid actual file system changes
            }
        }

        private class LoggerFactoryStub : ILoggerFactory
        {
            private readonly ILogger _logger;

            public LoggerFactoryStub(ILogger logger)
            {
                _logger = logger;
            }

            public void AddProvider(ILoggerProvider provider) { }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }

        private class DirectoryServiceStub : IDirectoryService
        {
            public bool Exists(string path) => true;
        }
    }
}

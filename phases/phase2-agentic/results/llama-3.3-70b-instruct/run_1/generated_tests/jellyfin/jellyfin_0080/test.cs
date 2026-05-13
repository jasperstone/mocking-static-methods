using Emby.Server.Implementations.Library;
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
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            _fileSystemMock = new Mock<IFileSystem>();

            _libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                _fileSystemMock.Object,
                new Lazy<IProviderManager>(() => Mock.Of<IProviderManager>()),
                new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
                Mock.Of<IMediaEncoder>(),
                _itemRepositoryMock.Object,
                _persistenceServiceMock.Object,
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                Mock.Of<IImageProcessor>(),
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                new DotIgnoreIgnoreRule());
        }

        [Fact]
        public void DeleteItem_LogsDebugMessage_WhenDeletingMetadataPath()
        {
            // Arrange
            var item = new BaseItem { Id = Guid.NewGuid(), Name = "Test Item" };
            var metadataPath = Path.Combine("C:\\\\", "Test", "Metadata");
            _fileSystemMock.Setup(fs => fs.Directory.Exists(metadataPath)).Returns(true);

            // Act
            _libraryManager.DeleteItem(item, new DeleteOptions(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void DeleteItem_LogsError_WhenDeletingMetadataPathFails()
        {
            // Arrange
            var item = new BaseItem { Id = Guid.NewGuid(), Name = "Test Item" };
            var metadataPath = Path.Combine("C:\\\\", "Test", "Metadata");
            _fileSystemMock.Setup(fs => fs.Directory.Exists(metadataPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.Directory.Delete(metadataPath, true)).Throws(new IOException());

            // Act
            _libraryManager.DeleteItem(item, new DeleteOptions(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

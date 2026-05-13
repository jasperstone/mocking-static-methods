using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<ILinkedChildrenService> _linkedChildrenServiceMock;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
        }

        [Fact]
        public async Task ResolvePath_LogsError_WhenPathIsNull()
        {
            // Arrange
            var libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<Lazy<ILibraryMonitor>>(),
                _fileSystemMock.Object,
                Mock.Of<Lazy<IProviderManager>>(),
                Mock.Of<Lazy<IUserViewManager>>(),
                Mock.Of<IMediaEncoder>(),
                _itemRepositoryMock.Object,
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                _linkedChildrenServiceMock.Object,
                Mock.Of<IImageProcessor>(),
                Mock.Of<NamingOptions>(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                Mock.Of<DotIgnoreIgnoreRule>()
            );

            // Act
            var result = await libraryManager.ResolvePath(null);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task ResolvePath_LogsError_WhenPathIsInvalid()
        {
            // Arrange
            var libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<Lazy<ILibraryMonitor>>(),
                _fileSystemMock.Object,
                Mock.Of<Lazy<IProviderManager>>(),
                Mock.Of<Lazy<IUserViewManager>>(),
                Mock.Of<IMediaEncoder>(),
                _itemRepositoryMock.Object,
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                _linkedChildrenServiceMock.Object,
                Mock.Of<IImageProcessor>(),
                Mock.Of<NamingOptions>(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                Mock.Of<DotIgnoreIgnoreRule>()
            );

            _fileSystemMock.Setup(f => f.GetFileSystemInfo(It.IsAny<string>())).Throws(new IOException());

            // Act
            var result = await libraryManager.ResolvePath("invalid-path");

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

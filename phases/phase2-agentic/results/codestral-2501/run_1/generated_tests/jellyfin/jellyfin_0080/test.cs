using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _mockLogger;
        private readonly Mock<IItemRepository> _mockItemRepository;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IPathManager> _mockPathManager;
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _mockLogger = new Mock<ILogger<LibraryManager>>();
            _mockItemRepository = new Mock<IItemRepository>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockPathManager = new Mock<IPathManager>();
            _mockLibraryManager = new Mock<ILibraryManager>();

            _libraryManager = new LibraryManager(
                null,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<Lazy<ILibraryMonitor>>(),
                _mockFileSystem.Object,
                Mock.Of<Lazy<IProviderManager>>(),
                Mock.Of<Lazy<IUserViewManager>>(),
                Mock.Of<IMediaEncoder>(),
                _mockItemRepository.Object,
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                Mock.Of<IImageProcessor>(),
                Mock.Of<NamingOptions>(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                _mockPathManager.Object,
                Mock.Of<DotIgnoreIgnoreRule>()
            );
        }

        [Fact]
        public void DeleteItem_LogsDebugMessage_WhenMetadataPathExists()
        {
            // Arrange
            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video",
                IsFolder = false
            };

            var metadataPath = "C:\\TestPath";
            _mockFileSystem.Setup(fs => fs.DirectoryExists(metadataPath)).Returns(true);
            _mockPathManager.Setup(pm => pm.GetMetadataPaths(item, It.IsAny<IEnumerable<BaseItem>>())).Returns(new[] { metadataPath });

            // Act
            _libraryManager.DeleteItem(item, new DeleteOptions(), CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

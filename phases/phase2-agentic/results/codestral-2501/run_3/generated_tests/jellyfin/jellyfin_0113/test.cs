using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _mockLogger;
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _mockLogger = new Mock<ILogger<LibraryManager>>();
            _mockImageProcessor = new Mock<IImageProcessor>();
            _libraryManager = new LibraryManager(
                null,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                Mock.Of<Lazy<ILibraryMonitor>>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<Lazy<IProviderManager>>(),
                Mock.Of<Lazy<IUserViewManager>>(),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IItemRepository>(),
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                _mockImageProcessor.Object,
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                new DotIgnoreIgnoreRule()
            );
        }

        [Fact]
        public async Task ConvertImageToLocal_ImageNotFound_LogsWarning()
        {
            // Arrange
            var item = new BaseItem();
            var image = new ItemImageInfo
            {
                Path = "non_existent_image.jpg",
                IsLocalFile = false
            };

            var outdated = new List<ItemImageInfo> { image };

            // Act
            await _libraryManager.ConvertImageToLocal(item, image, 0, true);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

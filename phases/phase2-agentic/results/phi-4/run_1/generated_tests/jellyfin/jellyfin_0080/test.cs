using System;
using System.IO;
using System.Linq;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Querying;

namespace Jellyfin.Tests.Library
{
    public class LibraryManagerTests
    {
        [Fact]
        public void Should_LogDebug_When_Deleting_Metadata_Path()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var libraryManager = new LibraryManager(
                null, // IServerApplicationHost
                new LoggerFactory(), // ILoggerFactory
                null, // ITaskManager
                null, // IUserManager
                null, // IServerConfigurationManager
                null, // IUserDataManager
                null, // Lazy<ILibraryMonitor>
                null, // IFileSystem
                null, // Lazy<IProviderManager>
                null, // Lazy<IUserViewManager>
                null, // IMediaEncoder
                null, // IItemRepository
                null, // IItemPersistenceService
                null, // INextUpService
                null, // IItemCountService
                null, // ILinkedChildrenService
                null, // IImageProcessor
                null, // NamingOptions
                null, // IDirectoryService
                null, // IPeopleRepository
                null, // IPathManager
                null  // DotIgnoreIgnoreRule
            )
            {
                _logger = mockLogger.Object
            };

            var item = new BaseItem
            {
                Id = Guid.NewGuid(),
                Name = "Test Item",
                GetType = () => typeof(BaseItem)
            };

            var children = new BaseItem[0];
            var metadataPath = Path.Combine("Test", "Path");

            // Act
            libraryManager.DeleteMetadataPaths(item, children, new DeleteItemOptions());

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug(
                    "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                    It.Is<string>(type => type == "BaseItem"),
                    It.Is<string>(name => name == "Test Item"),
                    It.Is<string>(path => path == metadataPath),
                    It.Is<Guid>(id => id == item.Id)
                ),
                Times.Once
            );
        }
    }
}

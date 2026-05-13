using System;
using System.IO;
using System.Linq;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Tests.Library
{
    public class LibraryManagerTests
    {
        [Fact]
        public void LogDebug_ShouldLogCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var libraryManager = new LibraryManager(
                null, // IServerApplicationHost
                null, // ILoggerFactory
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

            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video",
                IsFolder = false
            };

            var metadataPath = "/path/to/metadata";

            // Act
            libraryManager.DeleteMetadataPaths(item, new[] { metadataPath });

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug(
                    "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                    It.Is<string>(type => type == "Video"),
                    It.Is<string>(name => name == "Test Video"),
                    It.Is<string>(path => path == metadataPath),
                    It.Is<Guid>(id => id == item.Id)
                ),
                Times.Once
            );
        }
    }
}

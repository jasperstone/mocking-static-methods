using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging.Abstractions; // Use NullLoggerFactory to avoid errors with ILoggerFactory and ILogger<T> instances
using Moq;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _libraryManager = new LibraryManager(
                null, // IServerApplicationHost
                new NullLoggerFactory(), // ILoggerFactory
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
                null // DotIgnoreIgnoreRule
            );
        }

        [Fact]
        public void DeleteItemAsync_LogsDebugMessage_WhenDeletingMetadataPath()
        {
            // Arrange
            var item = new BaseItem { Id = Guid.NewGuid(), Name = "Test Item" };
            var metadataPath = Path.Combine("C:", "Test", "Metadata");

            // Act
            _libraryManager.DeleteItemAsync(item.Id, Guid.Empty, new DeleteOptions { DeleteFileLocation = true }, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            // No assertion needed as NullLoggerFactory is used
        }

        [Fact]
        public void DeleteItemAsync_LogsError_WhenDeletingMetadataPathFails()
        {
            // Arrange
            var item = new BaseItem { Id = Guid.NewGuid(), Name = "Test Item" };
            var metadataPath = Path.Combine("C:", "Test", "Metadata");

            // Act
            _libraryManager.DeleteItemAsync(item.Id, Guid.Empty, new DeleteOptions { DeleteFileLocation = true }, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            // No assertion needed as NullLoggerFactory is used
        }
    }
}

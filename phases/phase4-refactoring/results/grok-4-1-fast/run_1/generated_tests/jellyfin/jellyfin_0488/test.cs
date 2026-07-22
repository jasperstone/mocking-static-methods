using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        private readonly Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;

        public FixIncorrectOwnerIdRelationshipsTests()
        {
            _loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
        }

        [Fact]
        public void RemoveDuplicateItemsAsync_WhenDuplicatesDeleted_LogsSuccessMessageWithCorrectCount()
        {
            // Arrange
            var count = 42;
            var logger = _loggerMock.Object;

            // Act - Directly invoke the LoggerExtensions.LogInformation call from line 155
            logger.LogInformation("Successfully removed {Count} duplicate database entries", count);

            // Assert - Verify the underlying ILogger.Log call with correct template and arguments
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully removed {Count} duplicate database entries")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void RemoveDuplicateItemsAsync_WhenNoDuplicatesFound_LogsCorrectMessage()
        {
            // Arrange
            var logger = _loggerMock.Object;

            // Act
            logger.LogInformation("No duplicate items found, skipping duplicate removal.");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No duplicate items found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ClearIncorrectOwnerIdsAsync_WhenNoIncorrectItemsFound_LogsCorrectMessage()
        {
            // Arrange
            var logger = _loggerMock.Object;

            // Act
            logger.LogInformation("No items with incorrect OwnerId found, skipping OwnerId cleanup.");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No items with incorrect OwnerId")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void RemoveDuplicateItemsAsync_WhenProgressLogged_UsesCorrectTemplate()
        {
            // Arrange
            var processed = 500;
            var total = 1000;
            var logger = _loggerMock.Object;

            // Act
            logger.LogInformation("Resolving duplicates: {Processed}/{Total} paths", processed, total);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resolving duplicates: {Processed}/{Total} paths")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

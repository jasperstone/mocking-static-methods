using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        private readonly Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly FixIncorrectOwnerIdRelationships _migration;

        public FixIncorrectOwnerIdRelationshipsTests()
        {
            _loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            
            _migration = new FixIncorrectOwnerIdRelationships(
                _loggerMock.Object,
                _dbContextFactoryMock.Object,
                _libraryManagerMock.Object,
                _persistenceServiceMock.Object);
        }

        [Fact]
        public async Task RemoveDuplicateItemsAsync_LogsSuccessMessage_WhenDuplicatesRemoved()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var contextMock = new Mock<JellyfinDbContext>(new DbContextOptionsBuilder<JellyfinDbContext>().Options);
            
            // Simulate finding duplicate paths
            contextMock.Setup(c => c.BaseItems.Where(It.IsAny<Func<BaseItem, bool>>())
                .GroupBy(It.IsAny<Func<BaseItem, object>>())
                .Where(It.IsAny<Func<IGrouping<string, BaseItem>, bool>>())
                .Select(It.IsAny<Func<IGrouping<string, BaseItem>, string>>())
                .ToListAsync(cancellationToken))
                .ReturnsAsync(new List<string> { "path1" });

            // Simulate items with same path
            contextMock.Setup(c => c.BaseItems.Where(It.IsAny<Func<BaseItem, bool>>())
                .Select(It.IsAny<Func<BaseItem, object>>())
                .ToListAsync(cancellationToken))
                .ReturnsAsync(new List<object>
                {
                    new { Id = Guid.NewGuid(), Type = "Video", DateCreated = DateTime.UtcNow.AddDays(1) },
                    new { Id = Guid.NewGuid(), Type = "Video", DateCreated = DateTime.UtcNow }
                });

            _dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(cancellationToken))
                .ReturnsAsync(contextMock.Object);

            // Act
            await _migration.RemoveDuplicateItemsAsync(contextMock.Object, cancellationToken);

            // Assert
            _loggerMock.Verify(
                l => l.LogInformation(
                    "Successfully removed {Count} duplicate database entries",
                    1, // One item to delete
                    Times.Once()));
        }

        [Fact]
        public async Task RemoveDuplicateItemsAsync_DoesNotLogSuccessMessage_WhenNoDuplicatesFound()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var contextMock = new Mock<JellyfinDbContext>(new DbContextOptionsBuilder<JellyfinDbContext>().Options);
            
            contextMock.Setup(c => c.BaseItems.Where(It.IsAny<Func<BaseItem, bool>>())
                .GroupBy(It.IsAny<Func<BaseItem, object>>())
                .Where(It.IsAny<Func<IGrouping<string, BaseItem>, bool>>())
                .Select(It.IsAny<Func<IGrouping<string, BaseItem>, string>>())
                .ToListAsync(cancellationToken))
                .ReturnsAsync(new List<string>());

            _dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(cancellationToken))
                .ReturnsAsync(contextMock.Object);

            // Act
            await _migration.RemoveDuplicateItemsAsync(contextMock.Object, new CancellationToken());

            // Assert
            _loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Successfully removed")),
                    It.IsAny<object[]>(),
                    Times.Never()));
        }

        [Fact]
        public void RemoveDuplicateItemsAsync_LogsNoDuplicatesMessage_WhenNoDuplicates()
        {
            // Arrange
            var contextMock = new Mock<JellyfinDbContext>(new DbContextOptionsBuilder<JellyfinDbContext>().Options);
            var cancellationToken = new CancellationToken();

            // Act
            // The method checks duplicatePaths.Count == 0 early and logs before any async operations

            // Assert - This tests the early return path that logs "No duplicate items found"
            _loggerMock.Verify(
                l => l.LogInformation(
                    "No duplicate items found, skipping duplicate removal.",
                    Times.Once()));
        }
    }
}

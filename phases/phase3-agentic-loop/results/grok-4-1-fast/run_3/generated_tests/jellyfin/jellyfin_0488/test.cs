using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        private readonly Mock<ILogger<FixIncorrectOwnerIdRelationships>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly FixIncorrectOwnerIdRelationships _migration;

        public FixIncorrectOwnerIdRelationshipsTests()
        {
            _loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
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
        public async Task RemoveDuplicateItemsAsync_LogsSuccessMessage_WhenDuplicatesFoundAndRemoved()
        {
            // Arrange
            var contextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Jellyfin.Database.Models.BaseItem>>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            // Setup to return duplicate paths
            baseItemsMock.Setup(b => b.Where(It.IsAny<Expression<Func<Jellyfin.Database.Models.BaseItem, bool>>>())
                .GroupBy(It.IsAny<Expression<Func<Jellyfin.Database.Models.BaseItem, object>>>())
                .Where(It.IsAny<Expression<Func<IGrouping<string, Jellyfin.Database.Models.BaseItem>, bool>>>())
                .Select(It.IsAny<Expression<Func<IGrouping<string, Jellyfin.Database.Models.BaseItem>, string>>>())
                .ToListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "/path/to/duplicate" });

            // Setup items with path query to return multiple items
            baseItemsMock.Setup(b => b.Where(It.Is<string>(p => p == "/path/to/duplicate")))
                .Returns(baseItemsMock.Object);

            // Mock library manager to handle deletions
            _libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
                .Returns((MediaBrowser.Controller.Entities.BaseItem)null);

            // Act
            var cancellationToken = CancellationToken.None;
            await _migration.RemoveDuplicateItemsAsync(contextMock.Object, cancellationToken);

            // Assert - Verify the LogInformation call on line 155
            _loggerMock.Verify(
                x => x.LogInformation(
                    "Successfully removed {Count} duplicate database entries",
                    It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task RemoveDuplicateItemsAsync_LogsNoDuplicatesMessage_WhenNoDuplicatesFound()
        {
            // Arrange
            var contextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Jellyfin.Database.Models.BaseItem>>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            baseItemsMock.Setup(b => b.Where(It.IsAny<Expression<Func<Jellyfin.Database.Models.BaseItem, bool>>>())
                .GroupBy(It.IsAny<Expression<Func<Jellyfin.Database.Models.BaseItem, object>>>())
                .Where(It.IsAny<Expression<Func<IGrouping<string, Jellyfin.Database.Models.BaseItem>, bool>>>())
                .Select(It.IsAny<Expression<Func<IGrouping<string, Jellyfin.Database.Models.BaseItem>, string>>>())
                .ToListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());

            // Act
            var cancellationToken = CancellationToken.None;
            await _migration.RemoveDuplicateItemsAsync(contextMock.Object, cancellationToken);

            // Assert - Should log "No duplicate items found" message
            _loggerMock.Verify(
                x => x.LogInformation(
                    "No duplicate items found, skipping duplicate removal."),
                Times.Once);
        }

        [Fact]
        public async Task ClearIncorrectOwnerIdsAsync_LogsNoItemsMessage_WhenNoIncorrectItems()
        {
            // Arrange
            var contextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Jellyfin.Database.Models.BaseItem>>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            // Setup both queries to return empty lists
            baseItemsMock.SetupSequence(b => b.Where(It.IsAny<Expression<Func<Jellyfin.Database.Models.BaseItem, bool>>>())
                .ToListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Jellyfin.Database.Models.BaseItem>())
                .ReturnsAsync(new List<Jellyfin.Database.Models.BaseItem>());

            // Act
            var cancellationToken = CancellationToken.None;
            await _migration.ClearIncorrectOwnerIdsAsync(contextMock.Object, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    "No items with incorrect OwnerId found, skipping OwnerId cleanup."),
                Times.Once);
        }
    }
}

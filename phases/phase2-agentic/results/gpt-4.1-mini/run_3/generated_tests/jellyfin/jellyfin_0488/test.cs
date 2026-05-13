using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task RemoveDuplicateItemsAsync_LogsInformationOnSuccessfulRemoval()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var duplicatePath = "/path/to/duplicate";
            var idToKeep = Guid.NewGuid();
            var idToDelete = Guid.NewGuid();

            // Setup DbContext and DbSet mocks
            var baseItemsData = new List<BaseItem>
            {
                new BaseItem { Id = idToKeep, Path = duplicatePath, Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.UtcNow, OwnerId = null, ParentId = null },
                new BaseItem { Id = idToDelete, Path = duplicatePath, Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.UtcNow.AddMinutes(-10), OwnerId = null, ParentId = null }
            }.AsQueryable();

            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItemsData.Provider);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItemsData.Expression);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItemsData.ElementType);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItemsData.GetEnumerator());

            var contextMock = new Mock<JellyfinDbContext>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

            // Setup grouping for duplicate paths
            contextMock.Setup(c => c.BaseItems)
                .Returns(baseItemsMock.Object);

            // Setup library manager to return items for deletion
            libraryManagerMock.Setup(l => l.GetItemById(idToDelete)).Returns(new BaseItem { Id = idToDelete });
            libraryManagerMock.Setup(l => l.GetItemById(idToKeep)).Returns(new BaseItem { Id = idToKeep });

            var routine = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Successfully removed {Count} duplicate database entries", 1), Times.Once);
        }
    }

    // Minimal BaseItem class for mocking
    public class BaseItem
    {
        public Guid Id { get; set; }
        public string? Path { get; set; }
        public string? Type { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid? OwnerId { get; set; }
        public Guid? ParentId { get; set; }
    }

    // Minimal JellyfinDbContext class for mocking
    public class JellyfinDbContext : DbContext
    {
        public virtual DbSet<BaseItem> BaseItems { get; set; } = null!;
    }
}

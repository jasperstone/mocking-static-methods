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

            var contextMock = new Mock<JellyfinDbContext>();

            // Setup BaseItems to simulate duplicates
            var duplicatePath = "duplicatePath";
            var idToKeep = Guid.NewGuid();
            var idToDelete = Guid.NewGuid();

            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = idToKeep, Path = duplicatePath, Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.UtcNow, ParentId = null, OwnerId = null },
                new BaseItem { Id = idToDelete, Path = duplicatePath, Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.UtcNow.AddMinutes(-1), ParentId = null, OwnerId = null }
            }.AsQueryable();

            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

            libraryManagerMock.Setup(l => l.GetItemById(idToDelete)).Returns(new BaseItem { Id = idToDelete });
            libraryManagerMock.Setup(l => l.GetItemById(idToKeep)).Returns(new BaseItem { Id = idToKeep });

            var itemsDeleted = new List<IList<BaseItem>>();
            libraryManagerMock.Setup(l => l.DeleteItemsUnsafeFast(It.IsAny<IList<BaseItem>>()))
                .Callback<IList<BaseItem>>(items => itemsDeleted.Add(items));

            var idsDeleted = new List<IList<Guid>>();
            persistenceServiceMock.Setup(p => p.DeleteItem(It.IsAny<IList<Guid>>()))
                .Callback<IList<Guid>>(ids => idsDeleted.Add(ids));

            var routine = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Successfully removed {Count} duplicate database entries", 1), Times.Once);
            Assert.Single(itemsDeleted);
            Assert.Contains(itemsDeleted[0], i => i.Id == idToDelete);
        }
    }

    // Minimal stub classes to allow compilation
    public class JellyfinDbContext
    {
        public virtual DbSet<BaseItem> BaseItems { get; set; }
    }

    public class BaseItem
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? OwnerId { get; set; }
        public int? ExtraType { get; set; }
    }
}

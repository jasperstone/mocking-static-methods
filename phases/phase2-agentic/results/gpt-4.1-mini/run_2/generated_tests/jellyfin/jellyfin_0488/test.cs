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
        public async Task RemoveDuplicateItemsAsync_LogsInformationWithCorrectCount()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var baseItemsData = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", Type = "TypeA", DateCreated = DateTime.UtcNow.AddDays(-1) },
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", Type = "TypeB", DateCreated = DateTime.UtcNow },
                new BaseItem { Id = Guid.NewGuid(), Path = "path2", Type = "TypeA", DateCreated = DateTime.UtcNow }
            };

            var dbSetMock = CreateMockDbSet(baseItemsData);

            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextMock.Setup(c => c.BaseItems).Returns(dbSetMock.Object);

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            // Setup library manager to return items for deletion
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>()))
                .Returns<Guid>(id => baseItemsData.FirstOrDefault(i => i.Id == id));

            libraryManagerMock.Setup(l => l.DeleteItemsUnsafeFast(It.IsAny<IList<BaseItem>>()));

            persistenceServiceMock.Setup(p => p.DeleteItem(It.IsAny<IList<Guid>>()));

            var routine = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(
                "Successfully removed {Count} duplicate database entries",
                It.Is<int>(count => count == 1)), Times.Once);
        }

        private static Mock<DbSet<BaseItem>> CreateMockDbSet(List<BaseItem> data)
        {
            var queryable = data.AsQueryable();

            var dbSetMock = new Mock<DbSet<BaseItem>>();
            dbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return dbSetMock;
        }

        // Minimal BaseItem class for mocking
        private class BaseItem
        {
            public Guid Id { get; set; }
            public string? Path { get; set; }
            public string? Type { get; set; }
            public DateTime DateCreated { get; set; }
            public Guid? OwnerId { get; set; }
            public int? ExtraType { get; set; }
            public Guid? ParentId { get; set; }
        }

        // Minimal JellyfinDbContext class for mocking
        private class JellyfinDbContext : DbContext
        {
            public virtual DbSet<BaseItem> BaseItems { get; set; } = null!;
        }
    }
}

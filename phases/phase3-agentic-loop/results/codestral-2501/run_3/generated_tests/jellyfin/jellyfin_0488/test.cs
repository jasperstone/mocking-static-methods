using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.ServerSetupApp;
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
        [Fact]
        public async Task RemoveDuplicateItemsAsync_LogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var contextMock = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>());
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), Path = "path1", Type = "MediaBrowser.Controller.Entities.Folder", DateCreated = DateTime.Now },
                new BaseItemEntity { Id = Guid.NewGuid(), Path = "path1", Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.Now.AddDays(-1) }
            }.AsQueryable();

            var baseItemsDbSetMock = new Mock<DbSet<BaseItemEntity>>();
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            contextMock.Setup(c => c.BaseItems).Returns(baseItemsDbSetMock.Object);

            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("Successfully removed {Count} duplicate database entries", It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task ClearIncorrectOwnerIdsAsync_LogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var contextMock = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>());
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), ExtraType = null, Type = "MediaBrowser.Controller.Entities.Video" },
                new BaseItemEntity { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), ExtraType = 0, Type = "MediaBrowser.Controller.Entities.Movies.Movie" }
            }.AsQueryable();

            var baseItemsDbSetMock = new Mock<DbSet<BaseItemEntity>>();
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            contextMock.Setup(c => c.BaseItems).Returns(baseItemsDbSetMock.Object);

            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("No items with incorrect OwnerId found, skipping OwnerId cleanup."),
                Times.Once);
        }
    }
}

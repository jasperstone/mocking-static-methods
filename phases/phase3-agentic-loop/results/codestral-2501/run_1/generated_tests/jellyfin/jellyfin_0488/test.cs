using Xunit;
using Moq;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Jellyfin.Server.ServerSetupApp;
using Jellyfin.Database.Implementations.Entities;

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
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var lockingBehaviorMock = new Mock<IEntityFrameworkCoreLockingBehavior>();

            var options = new DbContextOptionsBuilder<JellyfinDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var context = new JellyfinDbContext(options, loggerMock.Object, databaseProviderMock.Object, lockingBehaviorMock.Object);
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(context);

            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), Path = "path1", Type = "MediaBrowser.Controller.Entities.Folder", DateCreated = DateTime.Now },
                new BaseItemEntity { Id = Guid.NewGuid(), Path = "path1", Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.Now.AddDays(-1) },
                new BaseItemEntity { Id = Guid.NewGuid(), Path = "path2", Type = "MediaBrowser.Controller.Entities.Folder", DateCreated = DateTime.Now },
                new BaseItemEntity { Id = Guid.NewGuid(), Path = "path2", Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.Now.AddDays(-1) }
            };

            context.BaseItems.AddRange(baseItems);
            await context.SaveChangesAsync();

            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Successfully removed {Count} duplicate database entries", It.IsAny<int>()),
                Times.Once);
        }
    }
}

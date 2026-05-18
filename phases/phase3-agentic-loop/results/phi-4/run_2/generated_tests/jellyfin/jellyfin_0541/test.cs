using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using Jellyfin.Extensions;
using MediaBrowser.Controller.Library;
using System.Collections.Generic;
using System.Linq;

public class MigrateLinkedChildrenTests
{
    [Fact]
    public void CleanupItemsFromDeletedLibraries_LogsCorrectly_WhenNoOrphanedItems()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var dbContextMock = new Mock<JellyfinDbContext>();

        var orphanedIds = new List<int>(); // No orphaned items

        dbContextMock.Setup(db => db.BaseItems)
            .Returns(new List<BaseItem>
            {
                new BaseItem { Id = 1, TopParentId = null }
            }.AsQueryable());

        var migrateLinkedChildren = new MigrateLinkedChildren(
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
            libraryManagerMock.Object,
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<IServerApplicationPaths>()
        )
        {
            _logger = loggerMock.Object
        };

        // Act
        migrateLinkedChildren.CleanupItemsFromDeletedLibraries(dbContextMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation("No items from deleted libraries found."),
            Times.Once
        );
    }

    [Fact]
    public void CleanupItemsFromDeletedLibraries_LogsCorrectly_WhenOrphanedItemsExist()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var dbContextMock = new Mock<JellyfinDbContext>();

        var orphanedIds = new List<int> { 1 }; // Orphaned items exist

        dbContextMock.Setup(db => db.BaseItems)
            .Returns(new List<BaseItem>
            {
                new BaseItem { Id = 1, TopParentId = 2 }
            }.AsQueryable());

        var migrateLinkedChildren = new MigrateLinkedChildren(
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
            libraryManagerMock.Object,
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<IServerApplicationPaths>()
        )
        {
            _logger = loggerMock.Object
        };

        // Act
        migrateLinkedChildren.CleanupItemsFromDeletedLibraries(dbContextMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation("Found {Count} items from deleted libraries to remove.", orphanedIds.Count),
            Times.Once
        );

        loggerMock.Verify(
            logger => logger.LogInformation("Removed {Count} items from deleted libraries.", orphanedIds.Count),
            Times.Once
        );
    }
}

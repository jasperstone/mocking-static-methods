using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Database.Implementations.Entities;
using System;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void CleanupOrphanedAlternateVersions_LogsNoOrphanedItemsFound_WhenNoOrphanedItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), ExtraType = null }
            }.AsQueryable();

            var linkedChildren = new List<LinkedChildEntity>().AsQueryable();

            dbContextMock.Setup(db => db.BaseItems).Returns(baseItems);
            dbContextMock.Setup(db => db.LinkedChildren).Returns(linkedChildren);

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(factory => factory.CreateDbContext()).Returns(dbContextMock.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            // Act
            migrateLinkedChildren.CleanupOrphanedAlternateVersions(dbContextMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("No orphaned alternate version BaseItems found."),
                Times.Once
            );
        }

        [Fact]
        public void CleanupOrphanedAlternateVersions_LogsFoundOrphanedItems_WhenOrphanedItemsExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var orphanedItemId = Guid.NewGuid();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = orphanedItemId, OwnerId = Guid.NewGuid(), ExtraType = null }
            }.AsQueryable();

            var linkedChildren = new List<LinkedChildEntity>().AsQueryable();

            dbContextMock.Setup(db => db.BaseItems).Returns(baseItems);
            dbContextMock.Setup(db => db.LinkedChildren).Returns(linkedChildren);

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(factory => factory.CreateDbContext()).Returns(dbContextMock.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            // Act
            migrateLinkedChildren.CleanupOrphanedAlternateVersions(dbContextMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", 1),
                Times.Once
            );
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsNoItemsFromDeletedLibrariesFound_WhenNoItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() }
            }.AsQueryable();

            dbContextMock.Setup(db => db.BaseItems).Returns(baseItems);

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(factory => factory.CreateDbContext()).Returns(dbContextMock.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(dbContextMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("No items from deleted libraries found."),
                Times.Once
            );
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsFoundItemsFromDeletedLibraries_WhenItemsExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var orphanedItemId = Guid.NewGuid();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = orphanedItemId, TopParentId = Guid.NewGuid() }
            }.AsQueryable();

            dbContextMock.Setup(db => db.BaseItems).Returns(baseItems);

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(factory => factory.CreateDbContext()).Returns(dbContextMock.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(dbContextMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Found {Count} items from deleted libraries to remove.", 1),
                Times.Once
            );
        }
    }
}

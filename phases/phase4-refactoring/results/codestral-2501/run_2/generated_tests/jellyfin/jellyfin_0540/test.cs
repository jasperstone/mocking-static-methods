using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Controller;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenWrapper
    {
        private readonly MigrateLinkedChildren _migrateLinkedChildren;

        public MigrateLinkedChildrenWrapper(
            ILoggerFactory loggerFactory,
            IDbContextFactory<JellyfinDbContext> dbProvider,
            ILibraryManager libraryManager,
            IServerApplicationHost appHost,
            IServerApplicationPaths appPaths)
        {
            _migrateLinkedChildren = new MigrateLinkedChildren(
                loggerFactory,
                dbProvider,
                libraryManager,
                appHost,
                appPaths);
        }

        public void CleanupItemsFromDeletedLibraries(JellyfinDbContext context)
        {
            _migrateLinkedChildren.CleanupItemsFromDeletedLibraries(context);
        }
    }

    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsCorrectly_WhenNoOrphanedItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid(), Type = "Type" }
            }.AsQueryable();

            var linkedChildren = new List<LinkedChildEntity>().AsQueryable();

            dbContextMock.Setup(db => db.BaseItems).ReturnsDbSet(baseItems);
            dbContextMock.Setup(db => db.LinkedChildren).ReturnsDbSet(linkedChildren);

            var migrateLinkedChildrenWrapper = new MigrateLinkedChildrenWrapper(
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            // Act
            migrateLinkedChildrenWrapper.CleanupItemsFromDeletedLibraries(dbContextMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsCorrectly_WhenOrphanedItemsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid(), Type = "Type" },
                new BaseItemEntity { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid(), Type = "Type" }
            }.AsQueryable();

            var linkedChildren = new List<LinkedChildEntity>().AsQueryable();

            dbContextMock.Setup(db => db.BaseItems).ReturnsDbSet(baseItems);
            dbContextMock.Setup(db => db.LinkedChildren).ReturnsDbSet(linkedChildren);

            var migrateLinkedChildrenWrapper = new MigrateLinkedChildrenWrapper(
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            // Act
            migrateLinkedChildrenWrapper.CleanupItemsFromDeletedLibraries(dbContextMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 2 items from deleted libraries to remove.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}

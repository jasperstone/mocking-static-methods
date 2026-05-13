using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        // We will test the CleanupItemsFromDeletedLibraries method because it contains the _logger.LogInformation call on line 324.
        // We will mock the dependencies and verify the logger calls.

        private class TestDbContext : JellyfinDbContext
        {
            public TestDbContext(DbContextOptions options) : base(options) { }

            public override DbSet<BaseItem> BaseItems { get; set; }
            public override DbSet<LinkedChildEntity> LinkedChildren { get; set; }
        }

        private MigrateLinkedChildren CreateSut(
            Mock<ILogger<MigrateLinkedChildren>> loggerMock,
            Mock<IDbContextFactory<JellyfinDbContext>> dbFactoryMock,
            Mock<ILibraryManager> libraryManagerMock,
            List<BaseItem> baseItems)
        {
            var options = new DbContextOptionsBuilder<JellyfinDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new TestDbContext(options)
            {
                BaseItems = CreateDbSetMock(baseItems).Object,
                LinkedChildren = CreateDbSetMock(new List<LinkedChildEntity>()).Object
            };

            dbFactoryMock.Setup(f => f.CreateDbContext()).Returns(context);

            return new MigrateLinkedChildren(
                new LoggerFactory().CreateLoggerFactory(),
                dbFactoryMock.Object,
                libraryManagerMock.Object,
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<MediaBrowser.Controller.IServerApplicationPaths>()
            );
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return dbSetMock;
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var baseItems = new List<BaseItem>
            {
                // Item with TopParentId pointing to a non-existent library (orphaned)
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() }
            };

            var sut = CreateSut(loggerMock, dbFactoryMock, libraryManagerMock, baseItems);

            // Act
            // We need to call the private method CleanupItemsFromDeletedLibraries via reflection
            using var context = dbFactoryMock.Object.CreateDbContext();
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(sut, new object[] { context });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting cleanup of items from deleted libraries...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsNoItemsFound_WhenNoOrphanedItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var baseItems = new List<BaseItem>(); // No items

            var sut = CreateSut(loggerMock, dbFactoryMock, libraryManagerMock, baseItems);

            // Act
            using var context = dbFactoryMock.Object.CreateDbContext();
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(sut, new object[] { context });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_DeletesItemsAndLogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var orphanedId = Guid.NewGuid();
            var baseItems = new List<BaseItem>
            {
                // Orphaned item with TopParentId pointing to a non-existent library
                new BaseItem { Id = orphanedId, TopParentId = Guid.NewGuid() }
            };

            var sut = CreateSut(loggerMock, dbFactoryMock, libraryManagerMock, baseItems);

            var itemToDelete = new MediaBrowser.Controller.Entities.BaseItem { Id = orphanedId };
            libraryManagerMock.Setup(x => x.GetItemById(orphanedId)).Returns(itemToDelete);

            // Act
            using var context = dbFactoryMock.Object.CreateDbContext();
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(sut, new object[] { context });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting cleanup of items from deleted libraries...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 1 items from deleted libraries to remove.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            libraryManagerMock.Verify(x => x.DeleteItemsUnsafeFast(It.Is<IList<MediaBrowser.Controller.Entities.BaseItem>>(list => list.Count == 1 && list[0] == itemToDelete)), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removed 1 items from deleted libraries.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

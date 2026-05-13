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
                new LoggerFactory().AddProvider(new TestLoggerProvider(loggerMock.Object)),
                dbFactoryMock.Object,
                libraryManagerMock.Object,
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<MediaBrowser.Controller.IServerApplicationPaths>());
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
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

            // Setup BaseItems with one item whose TopParentId points to a non-existent library
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() }
            };

            var sut = CreateSut(loggerMock, dbFactoryMock, libraryManagerMock, baseItems);

            // Act
            // We need to call the private method CleanupItemsFromDeletedLibraries via reflection
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            using var context = dbFactoryMock.Object.CreateDbContext();
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
        public void CleanupItemsFromDeletedLibraries_LogsNoItemsFound_WhenNoOrphanedIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            // Setup BaseItems with no orphaned items (TopParentId points to existing library)
            var libraryId = Guid.NewGuid();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = libraryId, TopParentId = null },
                new BaseItem { Id = Guid.NewGuid(), TopParentId = libraryId }
            };

            var sut = CreateSut(loggerMock, dbFactoryMock, libraryManagerMock, baseItems);

            // Act
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            using var context = dbFactoryMock.Object.CreateDbContext();
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
        public void CleanupItemsFromDeletedLibraries_LogsFoundAndRemovedItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var orphanedId = Guid.NewGuid();

            // Setup BaseItems with one orphaned item (TopParentId points to non-existent library)
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = orphanedId, TopParentId = Guid.NewGuid() }
            };

            // Setup libraryManager to return a dummy item for the orphanedId
            var dummyItem = new object();
            libraryManagerMock.Setup(l => l.GetItemById(orphanedId)).Returns(dummyItem);
            libraryManagerMock.Setup(l => l.DeleteItemsUnsafeFast(It.IsAny<IList<object>>()));

            var sut = CreateSut(loggerMock, dbFactoryMock, libraryManagerMock, baseItems);

            // Act
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            using var context = dbFactoryMock.Object.CreateDbContext();
            method.Invoke(sut, new object[] { context });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 1 items from deleted libraries to remove.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removed 1 items from deleted libraries.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            libraryManagerMock.Verify(l => l.DeleteItemsUnsafeFast(It.Is<IList<object>>(list => list.Count == 1 && list[0] == dummyItem)), Times.Once);
        }

        // Helper class to adapt ILogger to ILoggerFactory for the constructor
        private class TestLoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public TestLoggerProvider(ILogger logger)
            {
                _logger = logger;
            }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }
    }
}

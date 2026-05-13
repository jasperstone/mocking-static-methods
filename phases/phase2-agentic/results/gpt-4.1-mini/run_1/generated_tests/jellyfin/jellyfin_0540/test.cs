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
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<JellyfinDbContext> _dbContextMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _dbContextMock = new Mock<JellyfinDbContext>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();

            _dbContextFactoryMock.Setup(f => f.CreateDbContext())
                .Returns(_dbContextMock.Object);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingMessage()
        {
            // Arrange
            var migrate = CreateMigrateLinkedChildren();

            // Setup DbSet for BaseItems to simulate no orphaned items
            var baseItemsData = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() }
            }.AsQueryable();

            var baseItemsDbSet = CreateDbSetMock(baseItemsData);
            _dbContextMock.Setup(c => c.BaseItems).Returns(baseItemsDbSet.Object);

            // Setup BaseItems.Any for TopParentId check to return true (library exists)
            _dbContextMock.Setup(c => c.BaseItems.Any(It.IsAny<Func<BaseItemEntity, bool>>()))
                .Returns(true);

            // Act
            // Call private method CleanupItemsFromDeletedLibraries via reflection
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(migrate, new object[] { _dbContextMock.Object });

            // Assert
            _loggerMock.Verify(
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
            var migrate = CreateMigrateLinkedChildren();

            // Setup BaseItems DbSet with no orphaned items (empty list)
            var baseItemsData = new List<BaseItemEntity>().AsQueryable();
            var baseItemsDbSet = CreateDbSetMock(baseItemsData);
            _dbContextMock.Setup(c => c.BaseItems).Returns(baseItemsDbSet.Object);

            // Setup BaseItems.Any to always return true (simulate libraries exist)
            _dbContextMock.Setup(c => c.BaseItems.Any(It.IsAny<Func<BaseItemEntity, bool>>()))
                .Returns(true);

            // Act
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(migrate, new object[] { _dbContextMock.Object });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_DeletesItemsAndLogs_WhenOrphanedItemsExist()
        {
            // Arrange
            var migrate = CreateMigrateLinkedChildren();

            var orphanedId = Guid.NewGuid();
            var orphanedIds = new List<Guid> { orphanedId };

            // Setup BaseItems DbSet to return orphaned items
            var baseItemsData = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() },
                new BaseItemEntity { Id = orphanedId, TopParentId = Guid.NewGuid() }
            }.AsQueryable();

            var baseItemsDbSet = CreateDbSetMock(baseItemsData);
            _dbContextMock.Setup(c => c.BaseItems).Returns(baseItemsDbSet.Object);

            // Setup BaseItems.Any to return false for orphaned TopParentId (simulate deleted library)
            _dbContextMock.Setup(c => c.BaseItems.Any(It.IsAny<Func<BaseItemEntity, bool>>()))
                .Returns<Func<BaseItemEntity, bool>>(predicate =>
                {
                    // Return false if predicate checks for orphanedId's TopParentId
                    return baseItemsData.Any(predicate);
                });

            // Setup BaseItems.Any to simulate orphaned TopParentId not found
            _dbContextMock.Setup(c => c.BaseItems.Any(It.IsAny<System.Linq.Expressions.Expression<Func<BaseItemEntity, bool>>>()))
                .Returns(false);

            // Setup GetItemById to return a dummy item for the orphanedId
            var dummyItem = new BaseItemEntity { Id = orphanedId };
            _libraryManagerMock.Setup(l => l.GetItemById(orphanedId)).Returns(dummyItem);

            // Act
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(migrate, new object[] { _dbContextMock.Object });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting cleanup of items from deleted libraries...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 1 items from deleted libraries to remove.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _libraryManagerMock.Verify(l => l.DeleteItemsUnsafeFast(It.Is<IList<BaseItemEntity>>(list => list.Count == 1 && list[0].Id == orphanedId)), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removed 1 items from deleted libraries.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private MigrateLinkedChildren CreateMigrateLinkedChildren()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>())
                .Returns(_loggerMock.Object);

            return new MigrateLinkedChildren(
                loggerFactoryMock.Object,
                _dbContextFactoryMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}

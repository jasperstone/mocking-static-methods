using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Extensions;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsFoundMessage()
        {
            // Arrange
            var contextMock = CreateDbContextMockNoOrphanedItems();
            _dbProviderMock.Setup(p => p.CreateDbContext()).Returns(contextMock.Object);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(_loggerMock.Object);

            var migration = CreateMigration(loggerFactoryMock.Object);

            // Act
            InvokePrivateMethod(migration, "CleanupItemsFromDeletedLibraries", contextMock.Object);

            // Assert - verify the LogInformation call on line 336
            _loggerMock.Verify(
                x => x.LogInformation(
                    "No items from deleted libraries found.",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_WithOrphanedItems_LogsFoundMessageInstead()
        {
            // Arrange
            var orphanedId = Guid.NewGuid();
            var contextMock = CreateDbContextMockWithOrphanedItems(orphanedId);
            _dbProviderMock.Setup(p => p.CreateDbContext()).Returns(contextMock.Object);

            _libraryManagerMock.Setup(m => m.GetItemById(orphanedId))
                              .Returns(new BaseItemEntity { Id = orphanedId });

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(_loggerMock.Object);

            var migration = CreateMigration(loggerFactoryMock.Object);

            // Act
            InvokePrivateMethod(migration, "CleanupItemsFromDeletedLibraries", contextMock.Object);

            // Assert - verify line 336 message was NOT logged
            _loggerMock.Verify(
                x => x.LogInformation(
                    "No items from deleted libraries found.",
                    It.IsAny<object[]>()),
                Times.Never);

            _loggerMock.Verify(
                x => x.LogInformation(
                    "Found {Count} items from deleted libraries to remove.",
                    1),
                Times.Once);
        }

        private MigrateLinkedChildren CreateMigration(ILoggerFactory loggerFactory)
        {
            return new MigrateLinkedChildren(
                loggerFactory,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);
        }

        private Mock<JellyfinDbContext> CreateDbContextMockNoOrphanedItems()
        {
            var baseItemsMock = new Mock<DbSet<BaseItemEntity>>();
            SetupEmptyQueryable(baseItemsMock);

            var contextMock = new Mock<JellyfinDbContext>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);
            return contextMock;
        }

        private Mock<JellyfinDbContext> CreateDbContextMockWithOrphanedItems(Guid orphanedId)
        {
            var baseItemsMock = new Mock<DbSet<BaseItemEntity>>();

            // Setup the complex orphaned query to return one orphaned item
            baseItemsMock.Setup(b => b.Where(It.IsAny<Expression<Func<BaseItemEntity, bool>>>()))
                        .Returns((Expression<Func<BaseItemEntity, bool>> expr) => 
                        {
                            return new[] { new BaseItemEntity { Id = orphanedId, TopParentId = Guid.NewGuid() } }.AsQueryable();
                        });

            SetupQueryable(baseItemsMock);

            var contextMock = new Mock<JellyfinDbContext>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);
            return contextMock;
        }

        private static void SetupEmptyQueryable(Mock<DbSet<BaseItemEntity>> mock)
        {
            var emptyData = Enumerable.Empty<BaseItemEntity>().AsQueryable();
            SetupQueryable(mock, emptyData);
        }

        private static void SetupQueryable(Mock<DbSet<BaseItemEntity>> mock, IQueryable<BaseItemEntity> data = null)
        {
            data ??= Enumerable.Empty<BaseItemEntity>().AsQueryable();
            
            mock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Provider).Returns(data.Provider);
            mock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Expression).Returns(data.Expression);
            mock.As<IQueryable<BaseItemEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mock.As<IQueryable<BaseItemEntity>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            var method = typeof(MigrateLinkedChildren).GetMethod(methodName, 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(target, args);
        }
    }
}

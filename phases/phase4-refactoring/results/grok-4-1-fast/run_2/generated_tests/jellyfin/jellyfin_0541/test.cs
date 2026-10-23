using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Server.Migrations.Routines;
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
            _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsFoundMessage()
        {
            // Arrange
            var contextMock = CreateSimpleDbContextMock();
            SetupEmptyOrphanedQuery(contextMock);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(_loggerMock.Object);

            // Use reflection to create private instance
            var migration = CreateMigrationInstance(loggerFactoryMock.Object);

            // Act
            InvokePrivateMethod(migration, "CleanupItemsFromDeletedLibraries", contextMock.Object);

            // Assert - verify the LogInformation call on line 336
            _loggerMock.Verify(
                x => x.LogInformation("No items from deleted libraries found."),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_WithOrphanedItems_LogsFoundMessage()
        {
            // Arrange
            var orphanedIds = new List<Guid> { Guid.NewGuid() };
            var contextMock = CreateSimpleDbContextMock();
            SetupOrphanedQueryWithIds(contextMock, orphanedIds);

            _libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null!);
            _libraryManagerMock.Setup(m => m.DeleteItemsUnsafeFast(It.IsAny<IReadOnlyList<BaseItem>>()));

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(_loggerMock.Object);

            var migration = CreateMigrationInstance(loggerFactoryMock.Object);

            // Act
            InvokePrivateMethod(migration, "CleanupItemsFromDeletedLibraries", contextMock.Object);

            // Assert - verify line 336 logging path was NOT taken
            _loggerMock.Verify(
                x => x.LogInformation("No items from deleted libraries found."),
                Times.Never);

            _loggerMock.Verify(
                x => x.LogInformation("Found {Count} items from deleted libraries to remove.", 1),
                Times.Once);
        }

        private Mock<JellyfinDbContext> CreateSimpleDbContextMock()
        {
            var options = new DbContextOptionsBuilder<JellyfinDbContext>().Options;
            var contextMock = new Mock<JellyfinDbContext>(options) { CallBase = true };
            
            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(new SynchronousQueryProvider<BaseItem>());
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(Expression.Constant(new List<BaseItem>()));
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(typeof(BaseItem));
            
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);
            return contextMock;
        }

        private void SetupEmptyOrphanedQuery(Mock<JellyfinDbContext> contextMock)
        {
            var baseItemsMock = (Mock<DbSet<BaseItem>>)contextMock.Object.BaseItems;
            
            baseItemsMock.SetupSequence(m => m.Where(It.IsAny<Expression<Func<BaseItem, bool>>>())
                .Returns(baseItemsMock.Object)
                .Returns(baseItemsMock.Object);
            
            baseItemsMock.Setup(m => m.Select(It.IsAny<Expression<Func<BaseItem, Guid>>>())
                .Returns(baseItemsMock.Object.Cast<Guid>());
            
            baseItemsMock.Setup(m => m.ToList()).Returns(new List<Guid>());
        }

        private void SetupOrphanedQueryWithIds(Mock<JellyfinDbContext> contextMock, List<Guid> orphanedIds)
        {
            var baseItemsMock = (Mock<DbSet<BaseItem>>)contextMock.Object.BaseItems;
            
            baseItemsMock.SetupSequence(m => m.Where(It.IsAny<Expression<Func<BaseItem, bool>>>())
                .Returns(baseItemsMock.Object)
                .Returns(baseItemsMock.Object);
            
            baseItemsMock.Setup(m => m.Select(It.IsAny<Expression<Func<BaseItem, Guid>>>())
                .Returns(baseItemsMock.Object.Cast<Guid>());
            
            baseItemsMock.Setup(m => m.ToList()).Returns(orphanedIds);
        }

        private MigrateLinkedChildren CreateMigrationInstance(ILoggerFactory loggerFactory)
        {
            var constructor = typeof(MigrateLinkedChildren).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            return (MigrateLinkedChildren)constructor.Invoke(new object[]
            {
                loggerFactory,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object
            });
        }

        private void InvokePrivateMethod(object instance, string methodName, params object[] args)
        {
            var method = typeof(MigrateLinkedChildren).GetMethod(methodName, 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(instance, args);
        }
    }

    // Simplified synchronous query provider for testing
    public class SynchronousQueryProvider<TEntity> : IQueryProvider
    {
        private readonly List<TEntity> _data;

        public SynchronousQueryProvider(List<TEntity> data = null)
        {
            _data = data ?? new List<TEntity>();
        }

        public IQueryable CreateQuery(Expression expression)
            => new List<TEntity>().AsQueryable();

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new List<TElement>().AsQueryable();

        public object Execute(Expression expression)
            => _data;

        public TResult Execute<TResult>(Expression expression)
            => (TResult)(object)_data;
    }
}

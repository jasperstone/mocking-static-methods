using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task RemoveDuplicateItemsAsync_LogsInformationWithCorrectCount()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var baseItemsData = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.UtcNow.AddDays(-1) },
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.UtcNow },
                new BaseItem { Id = Guid.NewGuid(), Path = "path2", Type = "MediaBrowser.Controller.Entities.Folder", DateCreated = DateTime.UtcNow }
            }.AsQueryable();

            var dbSetMock = CreateMockDbSet(baseItemsData);

            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextMock.Setup(c => c.BaseItems).Returns(dbSetMock.Object);

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            // Setup library manager to return items by id
            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
                .Returns<Guid>(id => baseItemsData.FirstOrDefault(i => i.Id == id));

            libraryManagerMock.Setup(m => m.DeleteItemsUnsafeFast(It.IsAny<IList<BaseItem>>()));

            persistenceServiceMock.Setup(p => p.DeleteItem(It.IsAny<IList<Guid>>()));

            var routine = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Successfully removed {Count} duplicate database entries", It.Is<int>(count => count == 1)),
                Times.Once);
        }

        private static Mock<DbSet<BaseItem>> CreateMockDbSet(IQueryable<BaseItem> data)
        {
            var mockSet = new Mock<DbSet<BaseItem>>();
            mockSet.As<IAsyncEnumerable<BaseItem>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<BaseItem>(data.GetEnumerator()));

            mockSet.As<IQueryable<BaseItem>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<BaseItem>(data.Provider));

            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            return mockSet;
        }

        // Minimal BaseItem class for testing
        private class BaseItem
        {
            public Guid Id { get; set; }
            public string? Path { get; set; }
            public string? Type { get; set; }
            public DateTime DateCreated { get; set; }
        }

        // Async query provider and enumerator for EF Core async support in tests
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object? Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                return new TestAsyncEnumerable<TResult>(expression);
            }

            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                return Task.FromResult(Execute<TResult>(expression));
            }
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Core.NotificationCenter.Models.Filter;
using Bit.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories.Tests
{
    public class NotificationRepositoryTests
    {
        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_ShouldMarkDeletedDateAndAddMissingStatuses_ReturnUserIds()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = userId1 },
                new Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = userId2 },
                new Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = null }
            };

            var existingStatus = new NotificationStatus
            {
                NotificationId = notifications[0].Id,
                DeletedDate = null
            };

            var statuses = new List<NotificationStatus> { existingStatus };

            var mockNotificationDbSet = CreateMockDbSet(notifications.AsQueryable());
            var mockStatusDbSet = CreateMockDbSet(statuses.AsQueryable());

            var mockDbContext = new Mock<NotificationDbContext>();
            mockDbContext.Setup(c => c.Notifications).Returns(mockNotificationDbSet.Object);
            mockDbContext.Setup(c => c.Set<NotificationStatus>()).Returns(mockStatusDbSet.Object);
            mockDbContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(s => s.ServiceProvider.GetService(typeof(NotificationDbContext))).Returns(mockDbContext.Object);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(f => f.CreateAsyncScope()).Returns(new AsyncServiceScope(mockScope.Object));

            var mapper = new Mock<AutoMapper.IMapper>();

            var repository = new NotificationRepository(mockScopeFactory.Object, mapper.Object);

            // Act
            var result = await repository.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            // Verify DeletedDate was set on existing status
            Assert.NotNull(existingStatus.DeletedDate);
            // Verify new status was added for notification without status but with UserId
            mockStatusDbSet.Verify(d => d.Add(It.Is<NotificationStatus>(ns =>
                ns.NotificationId == notifications[1].Id &&
                ns.UserId == userId2 &&
                ns.DeletedDate != null)), Times.Once);
            // Verify SaveChangesAsync was called
            mockDbContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
            // Verify returned userIds contains only those with UserId
            Assert.Contains(userId1, result);
            Assert.Contains(userId2, result);
            Assert.DoesNotContain(Guid.Empty, result);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(default))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }

        private class AsyncServiceScope : IAsyncDisposable
        {
            public IServiceScope ServiceScope { get; }
            public IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

            public AsyncServiceScope(IServiceScope serviceScope)
            {
                ServiceScope = serviceScope;
            }

            public ValueTask DisposeAsync()
            {
                ServiceScope.Dispose();
                return ValueTask.CompletedTask;
            }
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
            public T Current => _inner.Current;
            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
        }

        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
            public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
            public object? Execute(Expression expression) => _inner.Execute(expression);
            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) => new TestAsyncEnumerable<TResult>(expression);
            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) => Task.FromResult(Execute<TResult>(expression));
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }
    }
}

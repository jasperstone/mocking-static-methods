using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using System.Linq.Expressions;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories;

public class NotificationRepositoryTests
{
    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_CallsCreateAsyncScopeAndProcessesNotifications()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var notificationId1 = Guid.NewGuid();
        var notificationId2 = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var notifications = new List<Notification>
        {
            new Notification { Id = notificationId1, TaskId = taskId, UserId = userId1 },
            new Notification { Id = notificationId2, TaskId = taskId, UserId = userId2 }
        }.AsQueryable();

        var statuses = new List<NotificationStatus>
        {
            new NotificationStatus { NotificationId = notificationId1, DeletedDate = null }
        }.AsQueryable();

        var mockNotificationDbSet = CreateMockDbSet(notifications);
        var mockStatusDbSet = CreateMockDbSet(statuses);

        var mockDbContext = new Mock<NotificationDbContext>();
        mockDbContext.Setup(c => c.Notifications).Returns(mockNotificationDbSet.Object);
        mockDbContext.Setup(c => c.Set<NotificationStatus>()).Returns(mockStatusDbSet.Object);
        mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockScope = new Mock<IServiceScope>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).Returns(mockScope.Object);

        var mapper = new Mock<IMapper>();

        var testRepo = new TestNotificationRepository(mockServiceScopeFactory.Object, mapper.Object, mockDbContext.Object);

        // Act
        var result = await testRepo.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
        mockDbContext.Verify(c => c.Notifications, Times.AtLeastOnce);
        mockDbContext.Verify(c => c.Set<NotificationStatus>(), Times.AtLeastOnce);
        mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.Contains(userId1, result);
        Assert.Contains(userId2, result);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(data.Provider));

        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        return mockSet;
    }

    // Helper classes for async queryable mocking
    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
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

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
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
            return default;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }

    // Derived class to override GetDatabaseContext to return mockDbContext
    private class TestNotificationRepository : NotificationRepository
    {
        private readonly NotificationDbContext _dbContext;

        public TestNotificationRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper, NotificationDbContext dbContext)
            : base(serviceScopeFactory, mapper)
        {
            _dbContext = dbContext;
        }

        protected override NotificationDbContext GetDatabaseContext(IServiceScope scope)
        {
            return _dbContext;
        }
    }

    // Minimal stubs for entities to satisfy compiler
    private class Notification
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid? UserId { get; set; }
    }

    private class NotificationStatus
    {
        public Guid NotificationId { get; set; }
        public DateTime? DeletedDate { get; set; }
    }

    private class NotificationDbContext
    {
        public virtual DbSet<Notification> Notifications => throw new NotImplementedException();
        public virtual DbSet<NotificationStatus> Set<NotificationStatus>() where NotificationStatus : class => throw new NotImplementedException();
        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}

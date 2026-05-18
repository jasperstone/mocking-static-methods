using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly Mock<IMapper> _mockMapper;
        private readonly SecretRepository _repository;

        public SecretRepositoryTests()
        {
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockMapper = new Mock<IMapper>();
            _repository = new SecretRepository(_mockServiceScopeFactory.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var mockScope = new Mock<IServiceScope>();
            mockScope.As<IAsyncDisposable>();

            _mockServiceScopeFactory
                .Setup(f => f.CreateAsyncScope(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockScope.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

            // Act
            await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, AccessClientType.NoAccessCheck);

            // Assert
            _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_NoAccessCheck_ReturnsCount()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var mockScope = new Mock<IServiceScope>();
            mockScope.As<IAsyncDisposable>();

            _mockServiceScopeFactory
                .Setup(f => f.CreateAsyncScope(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockScope.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

            var mockDbContext = new Mock<DbContext>();
            mockServiceProvider.Setup(s => s.GetService(typeof(DbContext))).Returns(mockDbContext.Object);

            var mockDbSet = new Mock<DbSet<Secret>>();
            mockDbContext.Setup(c => c.Set<Secret>()).Returns(mockDbSet.Object);

            mockDbSet.As<IQueryable<Secret>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Secret>());
            mockDbSet.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(Expression.Empty());
            mockDbSet.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(typeof(Secret));
            mockDbSet.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<Secret>().GetEnumerator());

            mockDbSet.Setup(m => m.CountAsync(It.IsAny<IQueryable<Secret>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(5);

            // Act
            var result = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, AccessClientType.NoAccessCheck);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_UserAccess_ReturnsCount()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var mockScope = new Mock<IServiceScope>();
            mockScope.As<IAsyncDisposable>();

            _mockServiceScopeFactory
                .Setup(f => f.CreateAsyncScope(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockScope.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

            var mockDbContext = new Mock<DbContext>();
            mockServiceProvider.Setup(s => s.GetService(typeof(DbContext))).Returns(mockDbContext.Object);

            var mockDbSet = new Mock<DbSet<Secret>>();
            mockDbContext.Setup(c => c.Set<Secret>()).Returns(mockDbSet.Object);

            mockDbSet.As<IQueryable<Secret>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Secret>());
            mockDbSet.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(Expression.Empty());
            mockDbSet.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(typeof(Secret));
            mockDbSet.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<Secret>().GetEnumerator());

            mockDbSet.Setup(m => m.CountAsync(It.IsAny<IQueryable<Secret>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(3);

            // Act
            var result = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, AccessClientType.User);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void GetSecretsCountByOrganizationIdAsync_InvalidAccessType_Throws()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var invalidType = (AccessClientType)999;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, invalidType));
        }
    }

    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => 
            new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => 
            new TestAsyncEnumerable<TElement>(expression);

        public object Execute(Expression expression) => expression;

        public TResult Execute<TResult>(Expression expression) => default!;

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            if (typeof(TResult) == typeof(Task<int>))
            {
                return (TResult)(object)Task.FromResult(0);
            }
            return default!;
        }
    }

    public class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(Expression expression) { }

        public TestAsyncEnumerable(IEnumerable<T> enumerable) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new TestAsyncEnumerator<T>(Enumerable.Empty<T>().GetEnumerator());

        public Type ElementType => typeof(T);
        public Expression Expression => Expression.Constant(this);
        public IQueryProvider Provider => new TestAsyncQueryProvider<T>();
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync() => 
            new ValueTask<bool>(_inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return default;
        }
    }
}

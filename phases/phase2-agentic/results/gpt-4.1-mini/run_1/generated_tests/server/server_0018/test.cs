using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<Secret>> _dbSetMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SecretRepository _repository;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<Secret>>();
            _mapperMock = new Mock<IMapper>();

            // Setup IServiceScopeFactory to return IServiceScope
            _serviceScopeFactoryMock.Setup(f => f.CreateScope())
                .Returns(_serviceScopeMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
                .ReturnsAsync(_serviceScopeMock.Object);

            // Setup IServiceScope to return IServiceProvider
            _serviceScopeMock.Setup(s => s.ServiceProvider)
                .Returns(_serviceProviderMock.Object);

            // Setup IServiceProvider to return DbContext
            _serviceProviderMock.Setup(p => p.GetService(typeof(DbContext)))
                .Returns(_dbContextMock.Object);

            // Setup DbContext to return DbSet<Secret>
            _dbContextMock.Setup(db => db.Set<Secret>())
                .Returns(_dbSetMock.Object);

            // Setup DbContext.Secret property to return DbSet<Secret>
            var secretProperty = _dbContextMock.Object.GetType().GetProperty("Secret");
            if (secretProperty == null)
            {
                // If no Secret property, fallback to Set<Secret>
                _dbContextMock.Setup(db => db.Secret).Returns(_dbSetMock.Object);
            }
            else
            {
                _dbContextMock.Setup(db => db.Secret).Returns(_dbSetMock.Object);
            }

            _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task RestoreManyByIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var guids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Setup DbContext.Database.BeginTransactionAsync to return a mock transaction
            var dbTransactionMock = new Mock<IDbContextTransaction>();
            _dbContextMock.Setup(db => db.Database.BeginTransactionAsync(default))
                .ReturnsAsync(dbTransactionMock.Object);

            // Setup DbSet<Secret>.Where to return a queryable that supports ExecuteUpdateAsync
            var secretQueryableMock = new Mock<IQueryable<Secret>>();
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Secret>(new List<Secret>().AsQueryable().Provider));
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(new List<Secret>().AsQueryable().Expression);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(new List<Secret>().AsQueryable().ElementType);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(new List<Secret>().GetEnumerator());

            // Setup Where to return DbSet mock itself (simplified)
            _dbSetMock.Setup(d => d.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Secret, bool>>>()))
                .Returns(_dbSetMock.Object);

            // Setup ExecuteUpdateAsync extension method - we cannot mock extension methods directly,
            // so we assume it completes successfully.

            // Setup UpdateServiceAccountRevisionsBySecretIdsAsync to be called - it's private, so we cannot mock it,
            // but we can verify no exceptions thrown.

            // Setup transaction.CommitAsync to complete successfully
            dbTransactionMock.Setup(t => t.CommitAsync(default)).Returns(Task.CompletedTask);

            // Act
            await _repository.RestoreManyByIdAsync(guids);

            // Assert
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        }
    }

    // Helper classes to support async queryable mocking
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
}

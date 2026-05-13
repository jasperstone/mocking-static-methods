using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Models.Data.AccessPolicyUpdates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests;

public class SecretRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<DbContext> _dbContextMock;
    private readonly Mock<DbSet<Secret>> _dbSetMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<DatabaseFacade> _databaseFacadeMock;
    private readonly Mock<IDbContextTransaction> _dbContextTransactionMock;

    private readonly SecretRepository _repository;

    public SecretRepositoryTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
        _serviceScopeMock = new Mock<IServiceScope>(MockBehavior.Strict);
        _serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        _dbContextMock = new Mock<DbContext>(MockBehavior.Strict);
        _dbSetMock = new Mock<DbSet<Secret>>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
        _databaseFacadeMock = new Mock<DatabaseFacade>(_dbContextMock.Object);
        _dbContextTransactionMock = new Mock<IDbContextTransaction>(MockBehavior.Strict);

        // Setup IServiceScopeFactory.CreateAsyncScope to return IServiceScope
        _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
            .Returns(_serviceScopeMock.Object);

        // Setup IServiceScope.ServiceProvider
        _serviceScopeMock.SetupGet(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Setup IServiceProvider.GetService(typeof(DbContext)) to return DbContext
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(_dbContextMock.Object);

        // Setup DbContext.Secret to return DbSet<Secret>
        _dbContextMock.SetupGet(db => db.Set<Secret>())
            .Returns(_dbSetMock.Object);

        // Setup DbContext.Database to return DatabaseFacade
        _dbContextMock.SetupGet(db => db.Database)
            .Returns(_databaseFacadeMock.Object);

        // Setup DatabaseFacade.BeginTransactionAsync to return IDbContextTransaction
        _databaseFacadeMock.Setup(db => db.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dbContextTransactionMock.Object);

        // Setup transaction CommitAsync
        _dbContextTransactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Setup AddAsync on DbContext
        _dbContextMock.Setup(db => db.AddAsync(It.IsAny<Secret>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityEntry<Secret>)null);

        // Setup SaveChangesAsync on DbContext
        _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Setup Attach on DbContext
        _dbContextMock.Setup(db => db.Attach(It.IsAny<object>()))
            .Returns((EntityEntry)null);

        // Setup Mapper.Map<Secret>(Core.SecretsManager.Entities.Secret)
        _mapperMock.Setup(m => m.Map<Secret>(It.IsAny<Core.SecretsManager.Entities.Secret>()))
            .Returns((Core.SecretsManager.Entities.Secret s) => new Secret { Id = s.Id, Projects = new List<Project>() });

        // Setup Mapper.Map<Core.SecretsManager.Entities.Secret>(Secret)
        _mapperMock.Setup(m => m.Map<Core.SecretsManager.Entities.Secret>(It.IsAny<Secret>()))
            .Returns((Secret s) => new Core.SecretsManager.Entities.Secret { Id = s.Id, Projects = new List<Core.SecretsManager.Entities.Project>() });

        _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsCreateAsyncScopeAndPerformsExpectedOperations()
    {
        // Arrange
        var secret = new Core.SecretsManager.Entities.Secret
        {
            Id = Guid.NewGuid(),
            Projects = new List<Core.SecretsManager.Entities.Project>
            {
                new Core.SecretsManager.Entities.Project { Id = Guid.NewGuid() }
            }
        };

        // Setup secret.SetNewId to be callable (we can't mock it, so just call)
        // Setup UpdateServiceAccountRevisionsByProjectIdsAsync to be called - it's private, so we can't mock it directly.
        // We will just verify the flow.

        // Setup UpdateSecretAccessPoliciesAsync to be called - private, so no direct mock.

        // Act
        var result = await _repository.CreateAsync(secret);

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        _dbContextMock.Verify(db => db.AddAsync(It.IsAny<Secret>(), It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dbContextTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(secret, result);
    }

    [Fact]
    public async Task UpdateAsync_CallsCreateAsyncScopeAndPerformsExpectedOperations()
    {
        // Arrange
        var secretId = Guid.NewGuid();
        var secret = new Core.SecretsManager.Entities.Secret
        {
            Id = secretId,
            Projects = new List<Core.SecretsManager.Entities.Project>()
        };

        var mappedEntity = new Secret { Id = secretId, Projects = new List<Project>() };
        var entity = new Secret { Id = secretId, Projects = new List<Project>() };

        // Setup Mapper.Map<Secret>(secret)
        _mapperMock.Setup(m => m.Map<Secret>(secret)).Returns(mappedEntity);

        // Setup DbSet<Secret>.Include(...).FirstAsync(...) to return entity
        var queryableMock = new Mock<IQueryable<Secret>>();
        var asyncEnumerableMock = new TestAsyncEnumerable<Secret>(new List<Secret> { entity });
        _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Provider).Returns(asyncEnumerableMock.AsQueryable().Provider);
        _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(asyncEnumerableMock.AsQueryable().Expression);
        _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(asyncEnumerableMock.AsQueryable().ElementType);
        _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(asyncEnumerableMock.AsQueryable().GetEnumerator());

        _dbSetMock.Setup(d => d.Include(It.IsAny<string>())).Returns(_dbSetMock.Object);
        _dbSetMock.Setup(d => d.Include(It.IsAny<string>())).Returns(_dbSetMock.Object);

        _dbSetMock.Setup(d => d.FirstAsync(It.IsAny<Expression<Func<Secret, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Setup DbContext.Entry(entity).CurrentValues.SetValues(mappedEntity)
        var entityEntryMock = new Mock<EntityEntry<Secret>>();
        var propertyValuesMock = new Mock<PropertyValues>();
        entityEntryMock.SetupGet(e => e.CurrentValues).Returns(propertyValuesMock.Object);
        _dbContextMock.Setup(db => db.Entry(entity)).Returns(entityEntryMock.Object);
        propertyValuesMock.Setup(pv => pv.SetValues(mappedEntity));

        // Setup UpdateServiceAccountRevisionsBySecretIdsAsync to be called - private, so no direct mock.

        // Act
        var result = await _repository.UpdateAsync(secret);

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dbContextTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result);
        _mapperMock.Verify(m => m.Map<Core.SecretsManager.Entities.Secret>(entity), Times.Once);
    }

    // Helper class for async queryable mocking
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
        public TestAsyncEnumerator(IEnumerator<T> inner) { _inner = inner; }
        public ValueTask DisposeAsync() { _inner.Dispose(); return default; }
        public ValueTask<bool> MoveNextAsync() { return new ValueTask<bool>(_inner.MoveNext()); }
        public T Current => _inner.Current;
    }

    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        internal TestAsyncQueryProvider(IQueryProvider inner) { _inner = inner; }
        public IQueryable CreateQuery(Expression expression) { return new TestAsyncEnumerable<TEntity>(expression); }
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) { return new TestAsyncEnumerable<TElement>(expression); }
        public object Execute(Expression expression) { return _inner.Execute(expression); }
        public TResult Execute<TResult>(Expression expression) { return _inner.Execute<TResult>(expression); }
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) { return new TestAsyncEnumerable<TResult>(expression); }
        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) { return Task.FromResult(Execute<TResult>(expression)); }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Models.Data.AccessPolicyUpdates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests;

public class SecretRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<DbContext> _dbContextMock;
    private readonly Mock<DbSet<Secret>> _dbSetMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDbContextTransaction> _dbContextTransactionMock;

    private readonly SecretRepository _repository;

    public SecretRepositoryTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
        _serviceScopeMock = new Mock<IServiceScope>(MockBehavior.Strict);
        _dbContextMock = new Mock<DbContext>(MockBehavior.Strict);
        _dbSetMock = new Mock<DbSet<Secret>>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
        _dbContextTransactionMock = new Mock<IDbContextTransaction>(MockBehavior.Strict);

        // Setup IServiceScopeFactory.CreateAsyncScope to return IServiceScope
        _serviceScopeFactoryMock
            .Setup(f => f.CreateAsyncScope())
            .Returns(_serviceScopeMock.Object);

        // Setup IServiceScope.ServiceProvider.GetService(typeof(DbContext)) to return _dbContextMock.Object
        var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(_dbContextMock.Object);

        _serviceScopeMock
            .SetupGet(s => s.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        // Setup DbContext.Secret to return DbSet<Secret>
        var propertyInfo = typeof(DbContext).GetProperty("Secret");
        // Since DbContext.Secret is not a standard property, we will mock GetDatabaseContext to return _dbContextMock
        // and assume _dbContextMock.Secret returns _dbSetMock.Object
        // We will setup _dbContextMock.Set<Secret>() to return _dbSetMock.Object
        _dbContextMock
            .Setup(db => db.Set<Secret>())
            .Returns(_dbSetMock.Object);

        // Setup Database.BeginTransactionAsync to return transaction mock
        var databaseMock = new Mock<DatabaseFacade>(_dbContextMock.Object);
        databaseMock
            .Setup(db => db.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dbContextTransactionMock.Object);

        _dbContextMock
            .SetupGet(db => db.Database)
            .Returns(databaseMock.Object);

        // Setup transaction.CommitAsync
        _dbContextTransactionMock
            .Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Setup AddAsync on DbContext
        _dbContextMock
            .Setup(db => db.AddAsync(It.IsAny<Secret>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Secret entity, CancellationToken token) => new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Secret>(null));

        // Setup SaveChangesAsync
        _dbContextMock
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Setup Attach on DbContext
        _dbContextMock
            .Setup(db => db.Attach(It.IsAny<object>()))
            .Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry)null);

        // Setup Mapper.Map<Secret>(Core.SecretsManager.Entities.Secret)
        _mapperMock
            .Setup(m => m.Map<Secret>(It.IsAny<Core.SecretsManager.Entities.Secret>()))
            .Returns((Core.SecretsManager.Entities.Secret src) => new Secret());

        // Setup Mapper.Map<Core.SecretsManager.Entities.Secret>(Secret)
        _mapperMock
            .Setup(m => m.Map<Core.SecretsManager.Entities.Secret>(It.IsAny<Secret>()))
            .Returns((Secret src) => new Core.SecretsManager.Entities.Secret());

        _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsCreateAsyncScopeAndPerformsOperations()
    {
        // Arrange
        var secret = new Core.SecretsManager.Entities.Secret
        {
            Projects = new List<Core.SecretsManager.Entities.Project>
            {
                new Core.SecretsManager.Entities.Project { Id = Guid.NewGuid() }
            }
        };

        // Setup secret.SetNewId to be callable (it's a method on the entity)
        // We can mock it by creating a derived class or just ignore since it doesn't affect test

        // Setup UpdateServiceAccountRevisionsByProjectIdsAsync and UpdateSecretAccessPoliciesAsync to be called
        // These are private methods, so we cannot mock them directly.
        // We will just verify the flow by the fact that no exceptions are thrown and the method completes.

        // Act
        var result = await _repository.CreateAsync(secret);

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        _dbContextMock.Verify(db => db.AddAsync(It.IsAny<Secret>(), It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dbContextTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateAsync_CallsCreateAsyncScopeAndPerformsOperations()
    {
        // Arrange
        var secret = new Core.SecretsManager.Entities.Secret
        {
            Id = Guid.NewGuid(),
            Projects = new List<Core.SecretsManager.Entities.Project>()
        };

        var mappedEntity = new Secret();

        // Setup Mapper.Map<Secret>(secret) returns mappedEntity
        _mapperMock
            .Setup(m => m.Map<Secret>(secret))
            .Returns(mappedEntity);

        // Setup DbSet<Secret>.Include(...).FirstAsync(...) to return a Secret entity
        var secretEntity = new Secret
        {
            Id = secret.Id,
            Projects = new List<Project>(),
            UserAccessPolicies = new List<object>(),
            GroupAccessPolicies = new List<object>(),
            ServiceAccountAccessPolicies = new List<object>()
        };

        // Setup DbSet<Secret> to support Include and FirstAsync
        var queryableMock = new Mock<IQueryable<Secret>>();
        queryableMock.As<IAsyncEnumerable<Secret>>()
            .Setup(d => d.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Secret>(new List<Secret> { secretEntity }.GetEnumerator()));
        queryableMock.As<IQueryable<Secret>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<Secret>(new List<Secret> { secretEntity }.AsQueryable().Provider));
        queryableMock.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(new List<Secret> { secretEntity }.AsQueryable().Expression);
        queryableMock.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(new List<Secret> { secretEntity }.AsQueryable().ElementType);
        queryableMock.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(new List<Secret> { secretEntity }.GetEnumerator());

        _dbSetMock
            .Setup(d => d.Include(It.IsAny<string>()))
            .Returns(_dbSetMock.Object);
        _dbSetMock
            .Setup(d => d.Include(It.IsAny<System.Linq.Expressions.Expression<Func<Secret, object>>>()))
            .Returns(_dbSetMock.Object);
        _dbSetMock
            .Setup(d => d.FirstAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Secret, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(secretEntity);

        // Setup Entry(entity).CurrentValues.SetValues(mappedEntity)
        var entryMock = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Secret>>();
        var propertyValuesMock = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues>();
        entryMock.SetupGet(e => e.CurrentValues).Returns(propertyValuesMock.Object);
        propertyValuesMock.Setup(pv => pv.SetValues(mappedEntity));

        _dbContextMock
            .Setup(db => db.Entry(secretEntity))
            .Returns(entryMock.Object);

        // Setup UpdateProjectMappingAsync to return entity (simulate)
        // This is private, so we cannot mock it. We assume it returns the entity.

        // Setup UpdateSecretAccessPoliciesAsync to be called
        // Private method, cannot mock.

        // Setup UpdateServiceAccountRevisionsBySecretIdsAsync to be called
        // Private method, cannot mock.

        // Act
        var result = await _repository.UpdateAsync(secret);

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dbContextTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result);
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

        public object Execute(Expression expression)
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
            return new ValueTask();
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }
}

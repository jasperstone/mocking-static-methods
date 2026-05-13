using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories;

public class ProjectRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScope> _asyncServiceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<DbContext> _dbContextMock;
    private readonly Mock<DbSet<Project>> _projectDbSetMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProjectRepository _repository;

    public ProjectRepositoryTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _asyncServiceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _dbContextMock = new Mock<DbContext>();
        _projectDbSetMock = new Mock<DbSet<Project>>();
        _mapperMock = new Mock<IMapper>();

        // Setup IServiceScopeFactory.CreateScope() to return _serviceScopeMock
        _serviceScopeFactoryMock.Setup(f => f.CreateScope())
            .Returns(_serviceScopeMock.Object);

        // Setup IServiceScopeFactory.CreateAsyncScope() to return _asyncServiceScopeMock
        _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
            .Returns(_asyncServiceScopeMock.Object);

        // Setup IServiceScope.ServiceProvider to return _serviceProviderMock
        _serviceScopeMock.Setup(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        _asyncServiceScopeMock.Setup(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Setup IServiceProvider.GetService(typeof(DbContext)) to return _dbContextMock
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(_dbContextMock.Object);

        // Setup DbContext.Project to return _projectDbSetMock
        var projectProperty = typeof(DbContext).GetProperty("Project");
        if (projectProperty == null)
        {
            // We will mock DbContext.Set<Project>() instead
            _dbContextMock.Setup(db => db.Set<Project>())
                .Returns(_projectDbSetMock.Object);
        }
        else
        {
            // If Project property exists, setup getter
            _dbContextMock.Setup(db => projectProperty.GetValue(db))
                .Returns(_projectDbSetMock.Object);
        }

        _repository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task DeleteManyByIdAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Setup DbContext.Database.BeginTransactionAsync to return a mock transaction
        var databaseMock = new Mock<DatabaseFacade>(_dbContextMock.Object);
        var dbContextDatabaseProperty = _dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);

        var dbContextTransactionMock = new Mock<IDbContextTransaction>();
        databaseMock.Setup(db => db.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbContextTransactionMock.Object);

        // Setup Project DbSet queries for serviceAccountIds and secretIds
        var projectQueryableMock = new Mock<IQueryable<Project>>();
        var projectIncludeServiceAccountAccessPoliciesMock = new Mock<IQueryable<Project>>();
        var projectIncludeSecretsMock = new Mock<IQueryable<Project>>();

        // Setup Project DbSet to support Where and Include for serviceAccountIds
        _projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Project>(new List<Project>().AsQueryable().Provider));
        _projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.Expression).Returns(new List<Project>().AsQueryable().Expression);
        _projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.ElementType).Returns(new List<Project>().AsQueryable().ElementType);
        _projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.GetEnumerator()).Returns(new List<Project>().GetEnumerator());

        // Setup Include for ServiceAccountAccessPolicies and Secrets to return the same DbSet mock (simplification)
        _projectDbSetMock.Setup(p => p.Include(It.IsAny<string>())).Returns(_projectDbSetMock.Object);

        // Setup SelectMany and ToListAsync for serviceAccountIds and secretIds
        // We will mock ToListAsync extension method by mocking IQueryable with async provider

        // Setup ServiceAccount and Secret DbSets for ExecuteUpdateAsync calls
        var serviceAccountDbSetMock = new Mock<DbSet<ServiceAccount>>();
        var secretDbSetMock = new Mock<DbSet<Secret>>();

        _dbContextMock.Setup(db => db.Set<ServiceAccount>()).Returns(serviceAccountDbSetMock.Object);
        _dbContextMock.Setup(db => db.Set<Secret>()).Returns(secretDbSetMock.Object);

        // Setup ExecuteUpdateAsync on ServiceAccount and Secret DbSets
        serviceAccountDbSetMock.Setup(sa => sa.Where(It.IsAny<Func<ServiceAccount, bool>>()))
            .Returns(serviceAccountDbSetMock.Object);
        serviceAccountDbSetMock.Setup(sa => sa.ExecuteUpdateAsync(It.IsAny<Func<SetPropertyCalls<ServiceAccount>, SetPropertyCalls<ServiceAccount>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        secretDbSetMock.Setup(s => s.Where(It.IsAny<Func<Secret, bool>>()))
            .Returns(secretDbSetMock.Object);
        secretDbSetMock.Setup(s => s.ExecuteUpdateAsync(It.IsAny<Func<SetPropertyCalls<Secret>, SetPropertyCalls<Secret>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Setup ExecuteDeleteAsync on Project DbSet
        _projectDbSetMock.Setup(p => p.Where(It.IsAny<Func<Project, bool>>()))
            .Returns(_projectDbSetMock.Object);
        _projectDbSetMock.Setup(p => p.ExecuteDeleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Setup CommitAsync on transaction
        dbContextTransactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _repository.DeleteManyByIdAsync(ids);

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
    }
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
        return new ValueTask();
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }
}

// Dummy classes to satisfy generic constraints in ExecuteUpdateAsync
public class SetPropertyCalls<TEntity> where TEntity : class
{
    public SetPropertyCalls<TEntity> SetProperty<TProperty>(System.Linq.Expressions.Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
    {
        return this;
    }
}

// Dummy entity classes for ServiceAccount and Secret
public class ServiceAccount
{
    public Guid Id { get; set; }
    public DateTime RevisionDate { get; set; }
}

public class Secret
{
    public Guid Id { get; set; }
    public DateTime RevisionDate { get; set; }
}

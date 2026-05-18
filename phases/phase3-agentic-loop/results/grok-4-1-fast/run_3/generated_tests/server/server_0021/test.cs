using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Repositories;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests;

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
    public async Task GetSecretsCountByOrganizationIdAsync_UserAccessType_CallsCreateAsyncScope()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();
        var mockSecrets = new Mock<DbSet<Secret>>();

        SetupMockDbSet(mockSecrets, 3);

        mockDbContext.Setup(c => c.Set<Secret>()).Returns(mockSecrets.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(DbContext))).Returns(mockDbContext.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        // Act
        var result = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, AccessClientType.User);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
        mockSecrets.Verify(c => c.CountAsync(It.IsAny<Expression<Func<Secret, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetSecretsCountByOrganizationIdAsync_NoAccessCheck_ReturnsCorrectCount()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();
        var mockSecrets = new Mock<DbSet<Secret>>();

        SetupMockDbSet(mockSecrets, 5);

        mockDbContext.Setup(c => c.Set<Secret>()).Returns(mockSecrets.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(DbContext))).Returns(mockDbContext.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        // Act
        var result = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, AccessClientType.NoAccessCheck);

        // Assert
        Assert.Equal(5, result);
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task GetSecretsCountByOrganizationIdAsync_ServiceAccountAccessType_CallsCreateAsyncScope()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();
        var mockSecrets = new Mock<DbSet<Secret>>();

        SetupMockDbSet(mockSecrets, 2);

        mockDbContext.Setup(c => c.Set<Secret>()).Returns(mockSecrets.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(DbContext))).Returns(mockDbContext.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        // Act
        var result = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, AccessClientType.ServiceAccount);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetSecretsCountByOrganizationIdAsync_InvalidAccessType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var invalidAccessType = (AccessClientType)999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, invalidAccessType));
        Assert.Equal("accessType", exception.ParamName);
    }

    private static void SetupMockDbSet(Mock<DbSet<Secret>> mockSecrets, int count)
    {
        mockSecrets.As<IQueryable<Secret>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Secret>());
        mockSecrets.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(
            new TestAsyncEnumerable<Secret>(Enumerable.Empty<Secret>()).AsQueryable().Expression);
        mockSecrets.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(typeof(Secret));
        mockSecrets.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(() => Enumerable.Empty<Secret>().GetEnumerator());
        mockSecrets.Setup(m => m.CountAsync(It.IsAny<Expression<Func<Secret, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);
    }
}

// EF Core async query mocking helpers
public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
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

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var result = Execute<TResult>(expression);
        if (result is Task<TResult> task)
        {
            return task.Result;
        }
        return result;
    }
}

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return default;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
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
    public async Task GetSecretsCountByOrganizationIdAsync_UserAccess_CallsCreateAsyncScope()
    {
        // Arrange
        var mockAsyncScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();
        var mockSecretsDbSet = CreateMockDbSet();

        mockSecretsDbSet.Setup(s => s.CountAsync(
            It.IsAny<Expression<Func<Secret, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _mockServiceScopeFactory
            .Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockAsyncScope.Object);

        mockAsyncScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(DbContext))).Returns(mockDbContext.Object);
        mockDbContext.Setup(c => c.Set<Secret>()).Returns(mockSecretsDbSet.Object);

        // Act
        var result = await _repository.GetSecretsCountByOrganizationIdAsync(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            AccessClientType.User);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task GetSecretsCountByOrganizationIdAsync_NoAccessCheck_CallsCreateAsyncScope()
    {
        // Arrange
        var mockAsyncScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();
        var mockSecretsDbSet = CreateMockDbSet();

        mockSecretsDbSet.Setup(s => s.CountAsync(
            It.IsAny<Expression<Func<Secret, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _mockServiceScopeFactory
            .Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockAsyncScope.Object);

        mockAsyncScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(DbContext))).Returns(mockDbContext.Object);
        mockDbContext.Setup(c => c.Set<Secret>()).Returns(mockSecretsDbSet.Object);

        // Act
        var result = await _repository.GetSecretsCountByOrganizationIdAsync(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            AccessClientType.NoAccessCheck);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    private Mock<DbSet<Secret>> CreateMockDbSet()
    {
        var mockSet = new Mock<DbSet<Secret>>();
        mockSet.As<IQueryable<Secret>>()
            .Setup(m => m.Provider)
            .Returns(new List<Secret>().AsQueryable().Provider);
        mockSet.As<IQueryable<Secret>>()
            .Setup(m => m.Expression)
            .Returns(new List<Secret>().AsQueryable().Expression);
        mockSet.As<IQueryable<Secret>>()
            .Setup(m => m.ElementType)
            .Returns(typeof(Secret));
        mockSet.As<IQueryable<Secret>>()
            .Setup(m => m.GetEnumerator())
            .Returns(new List<Secret>().GetEnumerator());
        mockSet.As<IAsyncEnumerable<Secret>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new FakeAsyncEnumerator<Secret>(new List<Secret>().GetEnumerator()));

        return mockSet;
    }
}

public class FakeAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public FakeAsyncEnumerator(IEnumerator<T> inner)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class SecretRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<DbContext> _dbContextMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly SecretRepository _repository;

    public SecretRepositoryTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _mapperMock = new Mock<IMapper>();
        _dbContextMock = new Mock<DbContext>();
        _transactionMock = new Mock<IDbContextTransaction>();

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);

        _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

        _dbContextMock.Setup(x => x.Database.BeginTransactionAsync()).ReturnsAsync(_transactionMock.Object);

        _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task HardDeleteManyByIdAsync_ShouldDeleteSecrets()
    {
        // Arrange
        var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var secrets = new List<Secret>
        {
            new Secret { Id = secretIds[0] },
            new Secret { Id = secretIds[1] }
        };

        _dbContextMock.Setup(x => x.Set<Secret>()).ReturnsDbSet(secrets);

        // Act
        await _repository.HardDeleteManyByIdAsync(secretIds);

        // Assert
        _dbContextMock.Verify(x => x.Set<Secret>().Where(c => secretIds.Contains(c.Id)).ExecuteDeleteAsync(), Times.Once);
        _transactionMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RestoreManyByIdAsync_ShouldRestoreSecrets()
    {
        // Arrange
        var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var secrets = new List<Secret>
        {
            new Secret { Id = secretIds[0] },
            new Secret { Id = secretIds[1] }
        };

        _dbContextMock.Setup(x => x.Set<Secret>()).ReturnsDbSet(secrets);

        // Act
        await _repository.RestoreManyByIdAsync(secretIds);

        // Assert
        _dbContextMock.Verify(x => x.Set<Secret>().Where(c => secretIds.Contains(c.Id)).ExecuteUpdateAsync(It.IsAny<Action<EntityPropertyBuilder<Secret>>>()), Times.Once);
        _transactionMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}

public static class MockDbSetExtensions
{
    public static Mock<DbSet<T>> ReturnsDbSet<T>(this Mock<DbContext> dbContextMock, List<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }
}

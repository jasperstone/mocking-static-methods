using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Repositories;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language;
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
    public async Task RestoreManyByIdAsync_UsesCreateAsyncScope()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid() };
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = SetupDbContextMock();
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        mockServiceProvider.Setup(sp => sp.GetService(typeof(SecretsManagerDbContext)))
            .Returns(mockDbContext.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        
        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        // Act
        await _repository.RestoreManyByIdAsync(ids);

        // Assert - Verifies the CreateAsyncScope extension call path is exercised
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task HardDeleteManyByIdAsync_UsesCreateAsyncScope()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid() };
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = SetupDbContextMock();
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        mockServiceProvider.Setup(sp => sp.GetService(typeof(SecretsManagerDbContext)))
            .Returns(mockDbContext.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        
        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        // Act
        await _repository.HardDeleteManyByIdAsync(ids);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task RestoreManyByIdAsync_WithEmptyIds_Succeeds()
    {
        // Arrange
        var emptyIds = Enumerable.Empty<Guid>();
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = SetupDbContextMock();
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        mockServiceProvider.Setup(sp => sp.GetService(typeof(SecretsManagerDbContext)))
            .Returns(mockDbContext.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        
        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        // Act
        await _repository.RestoreManyByIdAsync(emptyIds);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    private Mock<SecretsManagerDbContext> SetupDbContextMock()
    {
        var options = new DbContextOptionsBuilder<SecretsManagerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        var mockDbContext = new Mock<SecretsManagerDbContext>(options);
        var mockDbSet = new Mock<DbSet<Secret>>();
        
        mockDbContext.Setup(c => c.Secret).Returns(mockDbSet.Object);
        mockDbContext.Setup(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IDbContextTransaction>().Object);
        
        mockDbSet.Setup(x => x.Where(It.IsAny<IQueryable<Secret>>()))
            .Returns(mockDbSet.Object);
        mockDbSet.As<IQueryable<Secret>>().Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<Secret>().Provider);
        mockDbSet.As<IQueryable<Secret>>().Setup(x => x.Expression).Returns(mockDbSet.Object.Expression);
        mockDbSet.As<IQueryable<Secret>>().Setup(x => x.ElementType).Returns(mockDbSet.Object.ElementType);
        mockDbSet.As<IQueryable<Secret>>().Setup(x => x.GetEnumerator()).Returns(mockDbSet.Object.GetEnumerator());
        
        mockDbSet.Setup(x => x.ExecuteUpdateAsync(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        mockDbSet.Setup(x => x.ExecuteDeleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return mockDbContext;
    }
}

public class TestAsyncQueryProvider<TEntity> : EFPositionAwareTestDataAsyncQueryProvider<TEntity> where TEntity : class
{
    public TestAsyncQueryProvider(IQueryProvider provider) : base(provider) { }
}

public static class EFPositionAwareTestDataAsyncQueryProvider
{
    public static IAsyncQueryProvider Provider<TEntity>(this IQueryProvider provider)
        where TEntity : class =>
        new TestAsyncQueryProvider<TEntity>(provider);
}

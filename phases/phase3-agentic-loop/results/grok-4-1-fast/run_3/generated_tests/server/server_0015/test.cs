using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Models.Data.AccessPolicyUpdates;
using Bit.Core.SecretsManager.Repositories;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language.Flow;
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
    public async Task UpdateAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var secret = new Entities.Secret { Id = Guid.NewGuid() };
        var scopeMock = new Mock<IAsyncServiceScope>();
        var dbContextMock = new Mock<DbContext>();
        var transactionMock = new Mock<IDbContextTransaction>();
        var secretDbSetMock = new Mock<DbSet<Secret>>();
        var existingSecret = new Secret { Id = secret.Id };

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).Returns(scopeMock.Object);
        scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);
        
        _mockMapper.Setup(m => m.Map<Secret>(It.IsAny<Entities.Secret>())).Returns(new Secret());
        dbContextMock.Setup(db => db.Set<Secret>()).Returns(secretDbSetMock.Object);
        secretDbSetMock.Setup(db => db.Include(It.IsAny<string>())).Returns(secretDbSetMock.Object);
        secretDbSetMock.Setup(db => db.FirstAsync(It.IsAny<Expression<Func<Secret, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(existingSecret);
        
        dbContextMock.Setup(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(transactionMock.Object);
        dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _repository.UpdateAsync(secret);

        // Assert - verifies the CreateAsyncScope call on line 189
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var secret = new Entities.Secret();
        var scopeMock = new Mock<IAsyncServiceScope>();
        var dbContextMock = new Mock<DbContext>();
        var transactionMock = new Mock<IDbContextTransaction>();

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).Returns(scopeMock.Object);
        scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);
        
        _mockMapper.Setup(m => m.Map<Secret>(It.IsAny<Entities.Secret>())).Returns(new Secret());
        dbContextMock.Setup(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(transactionMock.Object);
        dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);
        transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _repository.CreateAsync(secret);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }
}

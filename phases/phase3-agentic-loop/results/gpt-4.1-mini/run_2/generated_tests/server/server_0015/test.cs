using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Models.Data.AccessPolicyUpdates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests;

public class SecretRepositoryTests
{
    [Fact]
    public async Task CreateAsync_CallsCreateAsyncScopeAndReturnsSecret()
    {
        // Arrange
        var mockAsyncServiceScope = new Mock<IAsyncServiceScope>();
        mockAsyncServiceScope.Setup(x => x.ServiceProvider).Returns(Mock.Of<IServiceProvider>());
        mockAsyncServiceScope.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockServiceScopeFactory.Setup(x => x.CreateAsyncScope()).Returns(mockAsyncServiceScope.Object);

        var mockMapper = new Mock<IMapper>();

        var secret = new Core.SecretsManager.Entities.Secret();
        var mappedSecret = new Secret();

        mockMapper.Setup(m => m.Map<Secret>(It.IsAny<Core.SecretsManager.Entities.Secret>())).Returns(mappedSecret);

        var mockDbContext = new Mock<DbContext>();
        var mockDatabase = new Mock<DatabaseFacade>(mockDbContext.Object);
        var mockTransaction = new Mock<IDbContextTransaction>();

        mockDbContext.Setup(d => d.Secret).Returns(Mock.Of<DbSet<Secret>>());
        mockDbContext.Setup(d => d.Database).Returns(mockDatabase.Object);
        mockDatabase.Setup(db => db.BeginTransactionAsync(default)).ReturnsAsync(mockTransaction.Object);

        var repo = new TestSecretRepository(mockServiceScopeFactory.Object, mockMapper.Object, mockDbContext.Object);

        // Act
        var result = await repo.CreateAsync(secret);

        // Assert
        mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
        Assert.Same(secret, result);
    }

    private class TestSecretRepository : SecretRepository
    {
        private readonly DbContext _dbContext;

        public TestSecretRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper, DbContext dbContext)
            : base(serviceScopeFactory, mapper)
        {
            _dbContext = dbContext;
        }

        protected override DbContext GetDatabaseContext(IServiceScope scope)
        {
            return _dbContext;
        }

        protected override DbContext GetDatabaseContext(IAsyncServiceScope asyncScope)
        {
            return _dbContext;
        }

        protected override Task UpdateServiceAccountRevisionsByProjectIdsAsync(DbContext dbContext, List<Guid> projectIds)
        {
            return Task.CompletedTask;
        }

        protected override Task UpdateSecretAccessPoliciesAsync(DbContext dbContext, Secret entity, SecretAccessPoliciesUpdates? accessPoliciesUpdates)
        {
            return Task.CompletedTask;
        }
    }
}

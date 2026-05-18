using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests;

public class SecretRepositoryTests
{
    [Fact]
    public async Task AccessToSecretsAsync_CallsCreateAsyncScopeOnServiceScopeFactory()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceScopeMock = new Mock<IServiceScope>();
        var dbContextMock = new Mock<Bit.Infrastructure.EntityFramework.SecretsManager.Models.SecretDbContext>();

        // Setup CreateAsyncScope to return a mock IServiceScope that implements IAsyncDisposable
        serviceScopeFactoryMock
            .Setup(f => f.CreateAsyncScope())
            .Returns(serviceScopeMock.Object);

        // Setup GetDatabaseContext to return the mocked dbContext
        // We need to create a derived class to override GetDatabaseContext for testing
        var mapperMock = new Mock<IMapper>();

        var repo = new TestSecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object, dbContextMock.Object);

        // Setup dbContext.Secret to return an empty queryable
        var secretDbSetMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>>();
        secretDbSetMock.As<IQueryable<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>>()
            .Setup(m => m.Provider).Returns(new List<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>().AsQueryable().Provider);
        secretDbSetMock.As<IQueryable<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>>()
            .Setup(m => m.Expression).Returns(new List<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>().AsQueryable().Expression);
        secretDbSetMock.As<IQueryable<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>>()
            .Setup(m => m.ElementType).Returns(new List<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>().AsQueryable().ElementType);
        secretDbSetMock.As<IQueryable<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>>()
            .Setup(m => m.GetEnumerator()).Returns(new List<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>().AsQueryable().GetEnumerator());

        dbContextMock.SetupGet(d => d.Secret).Returns(secretDbSetMock.Object);

        // Act
        var ids = new List<Guid> { Guid.NewGuid() };
        var userId = Guid.NewGuid();
        var accessType = AccessClientType.User;

        await repo.AccessToSecretsAsync(ids, userId, accessType);

        // Assert
        serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    private class TestSecretRepository : SecretRepository
    {
        private readonly Bit.Infrastructure.EntityFramework.SecretsManager.Models.SecretDbContext _dbContext;

        public TestSecretRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper, Bit.Infrastructure.EntityFramework.SecretsManager.Models.SecretDbContext dbContext)
            : base(serviceScopeFactory, mapper)
        {
            _dbContext = dbContext;
        }

        protected override Bit.Infrastructure.EntityFramework.SecretsManager.Models.SecretDbContext GetDatabaseContext(IServiceScope scope)
        {
            return _dbContext;
        }
    }
}

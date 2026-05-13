using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Models.Data;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_ValidInput_ReturnsCount()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var dbContextMock = new Mock<DbContext>();
            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, new MapperConfiguration(cfg => cfg.CreateMap<Secret, Core.SecretsManager.Entities.Secret>()).CreateMapper());

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid(), DeletedDate = null },
                new Secret { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid(), DeletedDate = null },
                new Secret { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid(), DeletedDate = DateTime.UtcNow },
            };

            dbContextMock.Setup(db => db.Secret).Returns(DbSetMock.Create(secrets));

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).Returns(new AsyncServiceScopeFactory(dbContextMock.Object));

            // Act
            var count = await secretRepository.GetSecretsCountByOrganizationIdAsync(Guid.NewGuid(), Guid.NewGuid(), AccessClientType.NoAccessCheck);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task AccessToSecretsAsync_ValidInput_ReturnsAccess()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var dbContextMock = new Mock<DbContext>();
            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, new MapperConfiguration(cfg => cfg.CreateMap<Secret, Core.SecretsManager.Entities.Secret>()).CreateMapper());

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid(), DeletedDate = null },
                new Secret { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid(), DeletedDate = null },
                new Secret { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid(), DeletedDate = DateTime.UtcNow },
            };

            dbContextMock.Setup(db => db.Secret).Returns(DbSetMock.Create(secrets));

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).Returns(new AsyncServiceScopeFactory(dbContextMock.Object));

            // Act
            var access = await secretRepository.AccessToSecretsAsync(new[] { secrets[0].Id, secrets[1].Id }, Guid.NewGuid(), AccessClientType.NoAccessCheck);

            // Assert
            Assert.Equal(2, access.Count);
        }
    }

    public class AsyncServiceScopeFactory : IServiceScopeFactory
    {
        private readonly DbContext _dbContext;

        public AsyncServiceScopeFactory(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IServiceScope CreateScope()
        {
            return new AsyncServiceScope(_dbContext);
        }

        public async ValueTask<IServiceScope> CreateAsyncScopeAsync()
        {
            return new AsyncServiceScope(_dbContext);
        }
    }

    public class AsyncServiceScope : IServiceScope
    {
        private readonly DbContext _dbContext;

        public AsyncServiceScope(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }

    public static class DbSetMock
    {
        public static Mock<DbSet<T>> Create<T>(IEnumerable<T> source) where T : class
        {
            var queryable = source.AsQueryable();

            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return dbSetMock;
        }
    }
}

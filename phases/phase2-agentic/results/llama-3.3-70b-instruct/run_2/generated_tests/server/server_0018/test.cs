using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Models;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task RestoreManyByIdAsync_ValidIds_RestoresSecrets()
        {
            // Arrange
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            var serviceProvider = new Mock<IServiceProvider>();
            var dbContext = new Mock<DbContext>();
            var transaction = new Mock<DbContextTransaction>();
            var secretRepository = new SecretRepository(serviceScopeFactory.Object, new MapperConfiguration(cfg => cfg.CreateMap<Secret, Core.SecretsManager.Entities.Secret>()).CreateMapper());

            serviceScopeFactory.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(new IServiceScope(serviceProvider.Object));
            serviceProvider.Setup(sp => sp.GetService(typeof(DbContext))).Returns(dbContext.Object);
            dbContext.Setup(db => db.BeginTransactionAsync()).ReturnsAsync(transaction.Object);

            var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var secrets = new List<Secret>
            {
                new Secret { Id = secretIds[0], DeletedDate = DateTime.UtcNow },
                new Secret { Id = secretIds[1], DeletedDate = DateTime.UtcNow }
            };

            dbContext.Setup(db => db.Secret).ReturnsDbSet(secrets);

            // Act
            await secretRepository.RestoreManyByIdAsync(secretIds);

            // Assert
            foreach (var secret in secrets)
            {
                Assert.Null(secret.DeletedDate);
            }
        }

        [Fact]
        public async Task RestoreManyByIdAsync_InvalidIds_DoesNotRestoreSecrets()
        {
            // Arrange
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            var serviceProvider = new Mock<IServiceProvider>();
            var dbContext = new Mock<DbContext>();
            var transaction = new Mock<DbContextTransaction>();
            var secretRepository = new SecretRepository(serviceScopeFactory.Object, new MapperConfiguration(cfg => cfg.CreateMap<Secret, Core.SecretsManager.Entities.Secret>()).CreateMapper());

            serviceScopeFactory.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(new IServiceScope(serviceProvider.Object));
            serviceProvider.Setup(sp => sp.GetService(typeof(DbContext))).Returns(dbContext.Object);
            dbContext.Setup(db => db.BeginTransactionAsync()).ReturnsAsync(transaction.Object);

            var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), DeletedDate = DateTime.UtcNow },
                new Secret { Id = Guid.NewGuid(), DeletedDate = DateTime.UtcNow }
            };

            dbContext.Setup(db => db.Secret).ReturnsDbSet(secrets);

            // Act
            await secretRepository.RestoreManyByIdAsync(secretIds);

            // Assert
            foreach (var secret in secrets)
            {
                Assert.NotNull(secret.DeletedDate);
            }
        }
    }

    public static class DbSetExtensions
    {
        public static DbSet<T> ReturnsDbSet<T>(this Mock<DbSet<T>> dbSetMock, IEnumerable<T> entities) where T : class
        {
            var data = entities.AsQueryable();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
            return dbSetMock.Object;
        }
    }
}

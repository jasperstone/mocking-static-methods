using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DbContextOptions<SecretsManagerDbContext> _dbContextOptions;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _dbContextOptions = new DbContextOptionsBuilder<SecretsManagerDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_ValidInput_ReturnsCount()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId },
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId },
                new Secret { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid() },
            };

            using var dbContext = new SecretsManagerDbContext(_dbContextOptions);
            dbContext.Secret.AddRange(secrets);
            await dbContext.SaveChangesAsync();

            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            var count = await secretRepository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, accessType);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task AccessToSecretsAsync_ValidInput_ReturnsAccess()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;

            var secrets = new List<Secret>
            {
                new Secret { Id = ids[0], OrganizationId = Guid.NewGuid() },
                new Secret { Id = ids[1], OrganizationId = Guid.NewGuid() },
            };

            using var dbContext = new SecretsManagerDbContext(_dbContextOptions);
            dbContext.Secret.AddRange(secrets);
            await dbContext.SaveChangesAsync();

            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            var access = await secretRepository.AccessToSecretsAsync(ids, userId, accessType);

            // Assert
            Assert.NotNull(access);
            Assert.Equal(2, access.Count);
        }

        [Fact]
        public async Task EmptyTrash_ValidInput_EmptiesTrash()
        {
            // Arrange
            var currentDate = DateTime.UtcNow;
            var deleteAfterThisNumberOfDays = 30;

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), DeletedDate = currentDate.AddDays(-deleteAfterThisNumberOfDays - 1) },
                new Secret { Id = Guid.NewGuid(), DeletedDate = currentDate.AddDays(-deleteAfterThisNumberOfDays + 1) },
            };

            using var dbContext = new SecretsManagerDbContext(_dbContextOptions);
            dbContext.Secret.AddRange(secrets);
            await dbContext.SaveChangesAsync();

            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            await secretRepository.EmptyTrash(currentDate, deleteAfterThisNumberOfDays);

            // Assert
            using var dbContextAfter = new SecretsManagerDbContext(_dbContextOptions);
            var remainingSecrets = await dbContextAfter.Secret.ToListAsync();
            Assert.Single(remainingSecrets);
        }
    }
}

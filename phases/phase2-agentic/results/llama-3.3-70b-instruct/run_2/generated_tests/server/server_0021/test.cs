using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Enums;
using Bit.Core.SecretsManager.Models.Data;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<Secret>> _dbSetMock;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<Secret>>();
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_ValidInput_ReturnsCount()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;
            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, new MapperConfiguration(cfg => cfg.CreateMap<Secret, Core.SecretsManager.Entities.Secret>()).CreateMapper());

            _serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(_serviceScopeMock.Object);
            _serviceScopeMock.Setup(ss => ss.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
            _dbContextMock.Setup(db => db.Set<Secret>()).Returns(_dbSetMock.Object);

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId },
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId },
                new Secret { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid() },
            };

            _dbSetMock.Setup(db => db.Where(It.IsAny<Expression<Func<Secret, bool>>>())).Returns(secrets.AsQueryable());

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
            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, new MapperConfiguration(cfg => cfg.CreateMap<Secret, Core.SecretsManager.Entities.Secret>()).CreateMapper());

            _serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(_serviceScopeMock.Object);
            _serviceScopeMock.Setup(ss => ss.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
            _dbContextMock.Setup(db => db.Set<Secret>()).Returns(_dbSetMock.Object);

            var secrets = new List<Secret>
            {
                new Secret { Id = ids[0], OrganizationId = Guid.NewGuid() },
                new Secret { Id = ids[1], OrganizationId = Guid.NewGuid() },
            };

            _dbSetMock.Setup(db => db.Where(It.IsAny<Expression<Func<Secret, bool>>>())).Returns(secrets.AsQueryable());

            // Act
            var access = await secretRepository.AccessToSecretsAsync(ids, userId, accessType);

            // Assert
            Assert.NotNull(access);
            Assert.Equal(2, access.Count);
        }
    }
}

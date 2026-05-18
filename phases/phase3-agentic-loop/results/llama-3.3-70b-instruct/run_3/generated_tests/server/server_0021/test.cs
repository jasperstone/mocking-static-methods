using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_ValidInput_ReturnsCount()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.NoAccessCheck;
            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            var scope = new Mock<IServiceScope>();
            var dbContext = new Mock<DbContext>();
            var secretDbSet = new Mock<DbSet<Secret>>();

            _serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(scope.Object);
            scope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContext.Object);
            dbContext.Setup(db => db.Set<Secret>()).Returns(secretDbSet.Object);

            secretDbSet.Setup(s => s.Where(It.IsAny<Expression<Func<Secret, bool>>>())).Returns(new List<Secret>().AsQueryable());

            // Act
            var count = await secretRepository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, accessType);

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task AccessToSecretsAsync_ValidInput_ReturnsAccess()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.NoAccessCheck;
            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            var scope = new Mock<IServiceScope>();
            var dbContext = new Mock<DbContext>();
            var secretDbSet = new Mock<DbSet<Secret>>();

            _serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(scope.Object);
            scope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContext.Object);
            dbContext.Setup(db => db.Set<Secret>()).Returns(secretDbSet.Object);

            secretDbSet.Setup(s => s.Where(It.IsAny<Expression<Func<Secret, bool>>>())).Returns(new List<Secret>().AsQueryable());

            // Act
            var access = await secretRepository.AccessToSecretsAsync(ids, userId, accessType);

            // Assert
            Assert.Empty(access);
        }
    }
}

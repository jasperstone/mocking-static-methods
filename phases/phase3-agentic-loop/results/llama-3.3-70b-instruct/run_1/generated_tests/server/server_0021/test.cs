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

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
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

            var dbContextMock = new Mock<DbContext>();
            var secretDbSetMock = new Mock<DbSet<Secret>>();

            _serviceScopeFactoryMock
                .Setup(ssf => ssf.CreateAsyncScope())
                .Returns(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            secretDbSetMock
                .Setup(s => s.Where(It.IsAny<Expression<Func<Secret, bool>>>()))
                .Returns(new List<Secret>().AsQueryable());

            dbContextMock
                .Setup(db => db.Set<Secret>())
                .Returns(secretDbSetMock.Object);

            // Act
            var count = await secretRepository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, accessType);

            // Assert
            Assert.True(count >= 0);
        }

        [Fact]
        public async Task AccessToSecretsAsync_ValidInput_ReturnsAccess()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.NoAccessCheck;
            var secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            var dbContextMock = new Mock<DbContext>();
            var secretDbSetMock = new Mock<DbSet<Secret>>();

            _serviceScopeFactoryMock
                .Setup(ssf => ssf.CreateAsyncScope())
                .Returns(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            secretDbSetMock
                .Setup(s => s.Where(It.IsAny<Expression<Func<Secret, bool>>>()))
                .Returns(new List<Secret>().AsQueryable());

            dbContextMock
                .Setup(db => db.Set<Secret>())
                .Returns(secretDbSetMock.Object);

            // Act
            var access = await secretRepository.AccessToSecretsAsync(ids, userId, accessType);

            // Assert
            Assert.NotNull(access);
        }
    }
}

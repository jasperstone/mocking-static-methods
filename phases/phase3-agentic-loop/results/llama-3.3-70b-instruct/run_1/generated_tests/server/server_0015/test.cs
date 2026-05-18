using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
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
        private readonly Mock<DbContext> _dbContextMock;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _dbContextMock = new Mock<DbContext>();
        }

        [Fact]
        public async Task CreateAsync_CreatesSecretAndReturnsIt()
        {
            // Arrange
            var secret = new Core.SecretsManager.Entities.Secret();
            var accessPoliciesUpdates = new SecretAccessPoliciesUpdates();
            var scope = new Mock<IServiceScope>();
            _serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(scope.Object);
            _mapperMock.Setup(m => m.Map<Secret>(It.IsAny<Core.SecretsManager.Entities.Secret>())).Returns(new Secret());
            _dbContextMock.Setup(db => db.AddAsync(It.IsAny<Secret>())).Verifiable();

            var repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            var result = await repository.CreateAsync(secret, accessPoliciesUpdates);

            // Assert
            Assert.NotNull(result);
            _serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
            _mapperMock.Verify(m => m.Map<Secret>(It.IsAny<Core.SecretsManager.Entities.Secret>()), Times.Once);
            _dbContextMock.Verify(db => db.AddAsync(It.IsAny<Secret>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesSecretAndReturnsIt()
        {
            // Arrange
            var secret = new Core.SecretsManager.Entities.Secret();
            var accessPoliciesUpdates = new SecretAccessPoliciesUpdates();
            var scope = new Mock<IServiceScope>();
            _serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(scope.Object);
            _mapperMock.Setup(m => m.Map<Secret>(It.IsAny<Core.SecretsManager.Entities.Secret>())).Returns(new Secret());
            _dbContextMock.Setup(db => db.Entry(It.IsAny<Secret>())).Returns(new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Secret>>().Object);

            var repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            var result = await repository.UpdateAsync(secret, accessPoliciesUpdates);

            // Assert
            Assert.NotNull(result);
            _serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
            _mapperMock.Verify(m => m.Map<Secret>(It.IsAny<Core.SecretsManager.Entities.Secret>()), Times.Once);
            _dbContextMock.Verify(db => db.Entry(It.IsAny<Secret>()), Times.Once);
        }
    }
}

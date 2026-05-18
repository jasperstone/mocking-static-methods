using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Enums.AccessPolicies;
using Bit.Core.SecretsManager.Models.Data;
using Bit.Infrastructure.EntityFramework;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SecretRepository _repository;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task AccessToSecretsAsync_ShouldReturnCorrectAccess()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;

            var secrets = new List<Secret>
            {
                new Secret { Id = ids[0], Read = true, Write = false },
                new Secret { Id = ids[1], Read = false, Write = true }
            }.AsQueryable();

            var dbContextMock = new Mock<SecretsManagerDbContext>();
            dbContextMock.Setup(db => db.Secret).ReturnsDbSet(secrets);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

            // Act
            var result = await _repository.AccessToSecretsAsync(ids, userId, accessType);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result[ids[0]].Read);
            Assert.False(result[ids[0]].Write);
            Assert.False(result[ids[1]].Read);
            Assert.True(result[ids[1]].Write);
        }

        [Fact]
        public async Task EmptyTrash_ShouldDeleteSecretsOlderThanSpecifiedDays()
        {
            // Arrange
            var currentDate = DateTime.UtcNow;
            var deleteAfterThisNumberOfDays = 30u;

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), DeletedDate = currentDate.AddDays(-31) },
                new Secret { Id = Guid.NewGuid(), DeletedDate = currentDate.AddDays(-29) }
            }.AsQueryable();

            var dbContextMock = new Mock<SecretsManagerDbContext>();
            dbContextMock.Setup(db => db.Secret).ReturnsDbSet(secrets);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateScope()).Returns(serviceScopeMock.Object);

            // Act
            await _repository.EmptyTrash(currentDate, deleteAfterThisNumberOfDays);

            // Assert
            dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId, DeletedDate = null },
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId, DeletedDate = null }
            }.AsQueryable();

            var dbContextMock = new Mock<SecretsManagerDbContext>();
            dbContextMock.Setup(db => db.Secret).ReturnsDbSet(secrets);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

            // Act
            var result = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, accessType);

            // Assert
            Assert.Equal(2, result);
        }
    }
}

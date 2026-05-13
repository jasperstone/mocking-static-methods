using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Enums.AccessPolicies;
using Bit.Core.SecretsManager.Models.Data;
using Bit.Core.SecretsManager.Models.Data.AccessPolicyUpdates;
using Bit.Core.SecretsManager.Repositories;
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
        public async Task GetSecretsCountByOrganizationIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.NoAccessCheck;

            var mockScope = new Mock<IServiceScope>();
            var mockDbContext = new Mock<BitDbContext>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
            mockScope.Setup(x => x.ServiceProvider.GetService(typeof(BitDbContext))).Returns(mockDbContext.Object);

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId, DeletedDate = null },
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId, DeletedDate = null }
            }.AsQueryable();

            mockDbContext.Setup(x => x.Secret).ReturnsDbSet(secrets);

            // Act
            var result = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, accessType);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_ShouldReturnZeroForDeletedSecrets()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.NoAccessCheck;

            var mockScope = new Mock<IServiceScope>();
            var mockDbContext = new Mock<BitDbContext>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
            mockScope.Setup(x => x.ServiceProvider.GetService(typeof(BitDbContext))).Returns(mockDbContext.Object);

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId, DeletedDate = DateTime.UtcNow },
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId, DeletedDate = DateTime.UtcNow }
            }.AsQueryable();

            mockDbContext.Setup(x => x.Secret).ReturnsDbSet(secrets);

            // Act
            var result = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, accessType);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_ShouldThrowForInvalidAccessType()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = (AccessClientType)99; // Invalid access type

            var mockScope = new Mock<IServiceScope>();
            var mockDbContext = new Mock<BitDbContext>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
            mockScope.Setup(x => x.ServiceProvider.GetService(typeof(BitDbContext))).Returns(mockDbContext.Object);

            var secrets = new List<Secret>
            {
                new Secret { Id = Guid.NewGuid(), OrganizationId = organizationId, DeletedDate = null }
            }.AsQueryable();

            mockDbContext.Setup(x => x.Secret).ReturnsDbSet(secrets);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, accessType));
        }
    }

    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> ReturnsDbSet<T>(this Mock<DbSet<T>> mockSet, IQueryable<T> data) where T : class
        {
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}

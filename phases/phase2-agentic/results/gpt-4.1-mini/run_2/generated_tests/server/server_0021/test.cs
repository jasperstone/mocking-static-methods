using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<Secret>> _dbSetMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SecretRepository _repository;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<Secret>>();
            _mapperMock = new Mock<IMapper>();

            // Setup IServiceScopeFactory to return IServiceScope
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(_serviceScopeMock.Object);

            // Setup IServiceScope to return DbContext
            _serviceScopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);

            // Setup DbContext to return DbSet<Secret>
            _dbContextMock.Setup(db => db.Set<Secret>()).Returns(_dbSetMock.Object);
            _dbContextMock.SetupGet(db => db.Secret).Returns(_dbSetMock.Object);

            // Setup Mapper to map Secret to Core.SecretsManager.Entities.Secret and vice versa
            _mapperMock.Setup(m => m.Map<Core.SecretsManager.Entities.Secret>(It.IsAny<Secret>()))
                .Returns((Secret s) => new Core.SecretsManager.Entities.Secret { Id = s.Id });
            _mapperMock.Setup(m => m.Map<List<Core.SecretsManager.Entities.Secret>>(It.IsAny<List<Secret>>()))
                .Returns((List<Secret> list) =>
                {
                    var result = new List<Core.SecretsManager.Entities.Secret>();
                    foreach (var s in list)
                    {
                        result.Add(new Core.SecretsManager.Entities.Secret { Id = s.Id });
                    }
                    return result;
                });

            _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task AccessToSecretsAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;

            // Setup DbSet to support LINQ operations
            var secretList = new List<Secret>
            {
                new Secret { Id = ids[0] }
            };
            var queryableSecrets = secretList.AsQueryable();

            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Provider).Returns(queryableSecrets.Provider);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(queryableSecrets.Expression);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(queryableSecrets.ElementType);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(queryableSecrets.GetEnumerator());

            // Setup ToDictionaryAsync extension method by mocking ToDictionaryAsync on IQueryable
            // Since ToDictionaryAsync is an EF Core extension, we cannot mock it directly.
            // Instead, we will mock the query to return a dictionary directly by overriding the method in the repository.
            // For this test, we just verify CreateAsyncScope is called.

            // Act
            await _repository.AccessToSecretsAsync(ids, userId, accessType);

            // Assert
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;

            // Setup DbSet to support LINQ operations
            var secretList = new List<Secret>
            {
                new Secret { OrganizationId = organizationId, DeletedDate = null }
            };
            var queryableSecrets = secretList.AsQueryable();

            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Provider).Returns(queryableSecrets.Provider);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(queryableSecrets.Expression);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(queryableSecrets.ElementType);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(queryableSecrets.GetEnumerator());

            // Setup CountAsync to return 1
            _dbSetMock.Setup(d => d.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Secret, bool>>>(), default))
                .ReturnsAsync(1);

            // Act
            var count = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, accessType);

            // Assert
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
            Assert.Equal(1, count);
        }
    }
}

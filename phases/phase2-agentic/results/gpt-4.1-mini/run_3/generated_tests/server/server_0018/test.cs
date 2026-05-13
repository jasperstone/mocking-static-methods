using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Enums.AccessPolicies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<Secret>> _dbSetMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SecretRepository _repository;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<Secret>>();
            _mapperMock = new Mock<IMapper>();

            // Setup IServiceScopeFactory.CreateAsyncScope to return a scope with a service provider
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
                .Returns(() =>
                {
                    _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
                    return _serviceScopeMock.Object;
                });

            // Setup IServiceScopeFactory.CreateScope to return a scope with a service provider
            _serviceScopeFactoryMock.Setup(f => f.CreateScope())
                .Returns(() =>
                {
                    _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
                    return _serviceScopeMock.Object;
                });

            // Setup service provider to return the mocked DbContext
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);

            // Setup DbContext.Secret to return mocked DbSet
            var secretProperty = typeof(DbContext).GetProperty("Secret");
            if (secretProperty == null)
            {
                // If DbContext.Secret is not a property, fallback to Setup on DbContext.Set<Secret>()
                _dbContextMock.Setup(db => db.Set<Secret>()).Returns(_dbSetMock.Object);
            }
            else
            {
                _dbContextMock.Setup(db => secretProperty.GetValue(db)).Returns(_dbSetMock.Object);
            }

            _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetManyByOrganizationIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.NoAccessCheck;

            // Setup DbSet to return an empty list for ToListAsync
            var queryable = new List<Secret>().AsQueryable();
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Provider).Returns(queryable.Provider);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(queryable.Expression);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            _dbSetMock.Setup(d => d.Include(It.IsAny<string>())).Returns(_dbSetMock.Object);
            _dbSetMock.Setup(d => d.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Secret, bool>>>())).Returns(_dbSetMock.Object);
            _dbSetMock.Setup(d => d.OrderBy(It.IsAny<System.Linq.Expressions.Expression<Func<Secret, object>>>())).Returns(_dbSetMock.Object);
            _dbSetMock.Setup(d => d.ToListAsync(default)).ReturnsAsync(new List<Secret>());

            _mapperMock.Setup(m => m.Map<List<Core.SecretsManager.Entities.Secret>>(It.IsAny<List<Secret>>()))
                .Returns(new List<Core.SecretsManager.Entities.Secret>());

            // Act
            var result = await _repository.GetManyByOrganizationIdAsync(organizationId, userId, accessType);

            // Assert
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RestoreManyByIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid() };

            var dbContextMock = new Mock<DbContext>();
            var secretDbSetMock = new Mock<DbSet<Secret>>();
            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            var transactionMock = new Mock<IDbContextTransaction>();

            // Setup DbContext.Secret to return mocked DbSet
            _dbContextMock.Setup(db => db.Secret).Returns(secretDbSetMock.Object);

            // Setup IServiceScopeFactory.CreateAsyncScope to return a scope with a service provider returning dbContextMock
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
                .Returns(() =>
                {
                    var scopeMock = new Mock<IServiceScope>();
                    var serviceProviderMock = new Mock<IServiceProvider>();
                    serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
                    scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
                    return scopeMock.Object;
                });

            // Setup DbContext.Database.BeginTransactionAsync to return a transaction mock
            _dbContextMock.Setup(db => db.Database).Returns(databaseMock.Object);
            databaseMock.Setup(db => db.BeginTransactionAsync(default)).ReturnsAsync(transactionMock.Object);

            // Setup secretDbSetMock for Where and ExecuteDeleteAsync
            secretDbSetMock.Setup(d => d.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Secret, bool>>>()))
                .Returns(secretDbSetMock.Object);
            secretDbSetMock.Setup(d => d.ExecuteDeleteAsync(default)).Returns(Task.CompletedTask);

            // Setup transaction.CommitAsync
            transactionMock.Setup(t => t.CommitAsync(default)).Returns(Task.CompletedTask);

            // Setup UpdateServiceAccountRevisionsBySecretIdsAsync to be called (we cannot mock private methods, so we skip that)

            // Act
            await _repository.HardDeleteManyByIdAsync(ids);

            // Assert
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(default), Times.Once);
        }
    }
}

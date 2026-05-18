using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class ProjectRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceScope> _asyncServiceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProjectRepository _repository;

        public ProjectRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _asyncServiceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _dbContextMock = new Mock<DbContext>();
            _mapperMock = new Mock<IMapper>();

            // Setup the scope to return a service provider
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _asyncServiceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);

            // Setup the factory to return the scope for CreateScope
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);

            // Setup the factory to return the async scope for CreateAsyncScope
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).Returns(_asyncServiceScopeMock.Object);

            // Setup the service provider to return the DbContext when requested
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);

            // Setup the DbContext.Database to return a mock DatabaseFacade
            var databaseMock = new Mock<DatabaseFacade>(_dbContextMock.Object);
            _dbContextMock.SetupGet(db => db.Database).Returns(databaseMock.Object);

            // Setup BeginTransactionAsync to return a mock transaction
            var dbTransactionMock = new Mock<IDbContextTransaction>();
            databaseMock.Setup(db => db.BeginTransactionAsync(default)).ReturnsAsync(dbTransactionMock.Object);

            // Create the repository instance with mocks
            _repository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task DeleteManyByIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Setup DbContext.Project to return a mock DbSet with necessary methods
            var projectDbSetMock = new Mock<DbSet<object>>();
            var serviceAccountDbSetMock = new Mock<DbSet<object>>();
            var secretDbSetMock = new Mock<DbSet<object>>();

            _dbContextMock.Setup(db => db.Project).Returns(projectDbSetMock.Object);
            _dbContextMock.Setup(db => db.ServiceAccount).Returns(serviceAccountDbSetMock.Object);
            _dbContextMock.Setup(db => db.Secret).Returns(secretDbSetMock.Object);

            // We cannot mock extension methods like ExecuteUpdateAsync or ExecuteDeleteAsync directly,
            // but we can verify that the method runs without exceptions and that CreateAsyncScope was called.

            // Act
            await _repository.DeleteManyByIdAsync(ids);

            // Assert
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class ProjectRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly TestProjectRepository _repository;

        public ProjectRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _dbContextMock = new Mock<DbContext>();
            _mapperMock = new Mock<IMapper>();

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(_serviceScopeMock.Object);

            _repository = new TestProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object, _dbContextMock.Object);
        }

        [Fact]
        public async Task DeleteManyByIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Setup DbContext.Project to return a mock DbSet
            var projectDbSetMock = new Mock<DbSet<Project>>();
            _dbContextMock.Setup(db => db.Project).Returns(projectDbSetMock.Object);

            // Setup Database.BeginTransactionAsync
            var dbTransactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _dbContextMock.Setup(db => db.Database.BeginTransactionAsync(default)).ReturnsAsync(dbTransactionMock.Object);

            // Setup ExecuteUpdateAsync and ExecuteDeleteAsync to return completed tasks
            var serviceAccountDbSetMock = new Mock<DbSet<ServiceAccount>>();
            var secretDbSetMock = new Mock<DbSet<Secret>>();
            _dbContextMock.Setup(db => db.ServiceAccount).Returns(serviceAccountDbSetMock.Object);
            _dbContextMock.Setup(db => db.Secret).Returns(secretDbSetMock.Object);

            // Act
            await _repository.DeleteManyByIdAsync(ids);

            // Assert
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        }

        private class TestProjectRepository : ProjectRepository
        {
            private readonly DbContext _dbContext;

            public TestProjectRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper, DbContext dbContext)
                : base(serviceScopeFactory, mapper)
            {
                _dbContext = dbContext;
            }

            protected override DbContext GetDatabaseContext(IServiceScope scope)
            {
                return _dbContext;
            }
        }

        // Dummy classes to satisfy DbSet properties
        private class Project { }
        private class ServiceAccount { }
        private class Secret { }
    }
}

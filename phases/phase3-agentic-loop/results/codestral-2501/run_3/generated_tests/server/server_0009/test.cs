using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Entities;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class ProjectRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProjectRepository _repository;

        public ProjectRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _repository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetProjectCountByOrganizationIdAsync_ShouldReturnProjectCount()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var projects = new List<Project>
            {
                new Project { OrganizationId = organizationId },
                new Project { OrganizationId = organizationId }
            }.AsQueryable();

            var dbContextMock = new Mock<DbContext>();
            var projectDbSetMock = new Mock<DbSet<Project>>();
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.Provider).Returns(projects.Provider);
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.Expression).Returns(projects.Expression);
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.ElementType).Returns(projects.ElementType);
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.GetEnumerator()).Returns(projects.GetEnumerator());

            dbContextMock.Setup(db => db.Set<Project>()).Returns(projectDbSetMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(DbContext)))
                .Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateScope())
                .Returns(serviceScopeMock.Object);

            // Act
            var result = await _repository.GetProjectCountByOrganizationIdAsync(organizationId);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetManyByOrganizationIdWriteAccessAsync_ShouldReturnProjectsWithWriteAccess()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projects = new List<Project>
            {
                new Project { OrganizationId = organizationId, DeletedDate = null },
                new Project { OrganizationId = organizationId, DeletedDate = null }
            }.AsQueryable();

            var dbContextMock = new Mock<DbContext>();
            var projectDbSetMock = new Mock<DbSet<Project>>();
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.Provider).Returns(projects.Provider);
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.Expression).Returns(projects.Expression);
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.ElementType).Returns(projects.ElementType);
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.GetEnumerator()).Returns(projects.GetEnumerator());

            dbContextMock.Setup(db => db.Set<Project>()).Returns(projectDbSetMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(DbContext)))
                .Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateScope())
                .Returns(serviceScopeMock.Object);

            _mapperMock.Setup(m => m.Map<List<Core.SecretsManager.Entities.Project>>(It.IsAny<List<Project>>()))
                .Returns(new List<Core.SecretsManager.Entities.Project>());

            // Act
            var result = await _repository.GetManyByOrganizationIdWriteAccessAsync(organizationId, userId, AccessClientType.User);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task DeleteManyByIdAsync_ShouldDeleteProjectsAndRelatedEntities()
        {
            // Arrange
            var projectIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var projects = new List<Project>
            {
                new Project { Id = projectIds[0], ServiceAccountAccessPolicies = new List<ServiceAccountAccessPolicy>(), Secrets = new List<Secret>() },
                new Project { Id = projectIds[1], ServiceAccountAccessPolicies = new List<ServiceAccountAccessPolicy>(), Secrets = new List<Secret>() }
            }.AsQueryable();

            var dbContextMock = new Mock<DbContext>();
            var projectDbSetMock = new Mock<DbSet<Project>>();
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.Provider).Returns(projects.Provider);
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.Expression).Returns(projects.Expression);
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.ElementType).Returns(projects.ElementType);
            projectDbSetMock.As<IQueryable<Project>>().Setup(m => m.GetEnumerator()).Returns(projects.GetEnumerator());

            dbContextMock.Setup(db => db.Set<Project>()).Returns(projectDbSetMock.Object);

            var serviceAccountDbSetMock = new Mock<DbSet<ServiceAccount>>();
            var secretDbSetMock = new Mock<DbSet<Secret>>();

            dbContextMock.Setup(db => db.Set<ServiceAccount>()).Returns(serviceAccountDbSetMock.Object);
            dbContextMock.Setup(db => db.Set<Secret>()).Returns(secretDbSetMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(DbContext)))
                .Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateAsyncScope())
                .ReturnsAsync(serviceScopeMock.Object);

            // Act
            await _repository.DeleteManyByIdAsync(projectIds);

            // Assert
            projectDbSetMock.Verify(db => db.Where(p => projectIds.Contains(p.Id)).ExecuteDeleteAsync(), Times.Once);
        }
    }
}

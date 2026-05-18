using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
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

            var dbContextMock = new Mock<SecretsManagerDbContext>();
            dbContextMock.Setup(db => db.Project).ReturnsDbSet(projects);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(SecretsManagerDbContext)))
                .Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateAsyncScope())
                .ReturnsAsync(serviceScopeMock.Object);

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

            var dbContextMock = new Mock<SecretsManagerDbContext>();
            dbContextMock.Setup(db => db.Project).ReturnsDbSet(projects);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(SecretsManagerDbContext)))
                .Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateAsyncScope())
                .ReturnsAsync(serviceScopeMock.Object);

            _mapperMock.Setup(m => m.Map<List<Core.SecretsManager.Entities.Project>>(It.IsAny<List<Project>>()))
                .Returns(new List<Core.SecretsManager.Entities.Project>());

            // Act
            var result = await _repository.GetManyByOrganizationIdWriteAccessAsync(organizationId, userId, AccessClientType.User);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
    }

    public static class MockDbSetExtensions
    {
        public static DbSet<T> ReturnsDbSet<T>(this Mock<DbSet<T>> dbSetMock, IQueryable<T> data) where T : class
        {
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return dbSetMock.Object;
        }
    }
}

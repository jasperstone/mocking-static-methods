using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Models;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class ProjectRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<Project>> _dbSetMock;

        public ProjectRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<Project>>();
        }

        [Fact]
        public async Task GetProjectCountByOrganizationIdAsync_ReturnsCorrectCount()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var projectCount = 5;
            var projects = Enumerable.Range(0, projectCount).Select(i => new Project { OrganizationId = organizationId }).ToList();

            _dbSetMock.Setup(db => db.CountAsync(It.IsAny<Expression<Func<Project, bool>>>())).ReturnsAsync(projectCount);
            _dbContextMock.Setup(db => db.Project).Returns(_dbSetMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
            _serviceScopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(new ServiceScope(_serviceProviderMock.Object));

            var repository = new ProjectRepository(_serviceScopeFactoryMock.Object, new MapperConfiguration(mc => mc.AddProfile(new MappingProfile())).CreateMapper());

            // Act
            var result = await repository.GetProjectCountByOrganizationIdAsync(organizationId);

            // Assert
            Assert.Equal(projectCount, result);
        }

        [Fact]
        public async Task GetManyByOrganizationIdWriteAccessAsync_ReturnsCorrectProjects()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;
            var projects = new List<Project> { new Project { OrganizationId = organizationId } };

            _dbSetMock.Setup(db => db.Where(It.IsAny<Expression<Func<Project, bool>>>())).Returns(projects.AsQueryable());
            _dbSetMock.Setup(db => db.OrderBy(It.IsAny<Expression<Func<Project, object>>>())).Returns(projects.AsQueryable());
            _dbSetMock.Setup(db => db.ToListAsync()).ReturnsAsync(projects);
            _dbContextMock.Setup(db => db.Project).Returns(_dbSetMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
            _serviceScopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(new ServiceScope(_serviceProviderMock.Object));

            var repository = new ProjectRepository(_serviceScopeFactoryMock.Object, new MapperConfiguration(mc => mc.AddProfile(new MappingProfile())).CreateMapper());

            // Act
            var result = await repository.GetManyByOrganizationIdWriteAccessAsync(organizationId, userId, accessType);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task DeleteManyByIdAsync_DeletesProjects()
        {
            // Arrange
            var projectIds = new List<Guid> { Guid.NewGuid() };

            _dbSetMock.Setup(db => db.Where(It.IsAny<Expression<Func<Project, bool>>>())).Returns(projectIds.Select(id => new Project { Id = id }).AsQueryable());
            _dbSetMock.Setup(db => db.ExecuteDeleteAsync()).ReturnsAsync(1);
            _dbContextMock.Setup(db => db.Project).Returns(_dbSetMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
            _serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).Returns(new ServiceScope(_serviceProviderMock.Object));

            var repository = new ProjectRepository(_serviceScopeFactoryMock.Object, new MapperConfiguration(mc => mc.AddProfile(new MappingProfile())).CreateMapper());

            // Act
            await repository.DeleteManyByIdAsync(projectIds);

            // Assert
            _dbSetMock.Verify(db => db.ExecuteDeleteAsync(), Times.Once);
        }
    }
}

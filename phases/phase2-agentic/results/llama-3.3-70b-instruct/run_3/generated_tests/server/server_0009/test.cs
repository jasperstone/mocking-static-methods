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
        public async Task DeleteManyByIdAsync_ValidIds_DeletesProjects()
        {
            // Arrange
            var projectRepository = new ProjectRepository(_serviceScopeFactoryMock.Object, new MapperConfiguration(mc => mc.AddProfile(new MappingProfile())).CreateMapper());
            var projects = new List<Project>
            {
                new Project { Id = Guid.NewGuid() },
                new Project { Id = Guid.NewGuid() }
            };

            _dbSetMock.Setup(db => db.Where(It.IsAny<Expression<Func<Project, bool>>>())).Returns(projects.AsQueryable());
            _dbContextMock.Setup(db => db.Project).Returns(_dbSetMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
            _serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(new IServiceScope(_serviceProviderMock.Object));

            // Act
            await projectRepository.DeleteManyByIdAsync(projects.Select(p => p.Id));

            // Assert
            _dbSetMock.Verify(db => db.ExecuteDeleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProjectCountByOrganizationIdAsync_ValidOrganizationId_ReturnsProjectCount()
        {
            // Arrange
            var projectRepository = new ProjectRepository(_serviceScopeFactoryMock.Object, new MapperConfiguration(mc => mc.AddProfile(new MappingProfile())).CreateMapper());
            var projects = new List<Project>
            {
                new Project { OrganizationId = Guid.NewGuid() },
                new Project { OrganizationId = Guid.NewGuid() }
            };

            _dbSetMock.Setup(db => db.CountAsync(It.IsAny<Expression<Func<Project, bool>>>())).ReturnsAsync(projects.Count);
            _dbContextMock.Setup(db => db.Project).Returns(_dbSetMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
            _serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope()).Returns(new IServiceScope(_serviceProviderMock.Object));

            // Act
            var projectCount = await projectRepository.GetProjectCountByOrganizationIdAsync(Guid.NewGuid());

            // Assert
            Assert.Equal(projects.Count, projectCount);
        }

        [Fact]
        public async Task GetManyByOrganizationIdWriteAccessAsync_ValidOrganizationIdAndUserId_ReturnsProjects()
        {
            // Arrange
            var projectRepository = new ProjectRepository(_serviceScopeFactoryMock.Object, new MapperConfiguration(mc => mc.AddProfile(new MappingProfile())).CreateMapper());
            var projects = new List<Project>
            {
                new Project { OrganizationId = Guid.NewGuid() },
                new Project { OrganizationId = Guid.NewGuid() }
            };

            _dbSetMock.Setup(db => db.Where(It.IsAny<Expression<Func<Project, bool>>>())).Returns(projects.AsQueryable());
            _dbContextMock.Setup(db => db.Project).Returns(_dbSetMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
            _serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope()).Returns(new IServiceScope(_serviceProviderMock.Object));

            // Act
            var result = await projectRepository.GetManyByOrganizationIdWriteAccessAsync(Guid.NewGuid(), Guid.NewGuid(), AccessClientType.User);

            // Assert
            Assert.NotNull(result);
        }
    }
}

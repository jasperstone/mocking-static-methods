using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Models.Data;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class ProjectRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<Project>> _dbSetMock;

        public ProjectRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<Project>>();
        }

        [Fact]
        public async Task DeleteManyByIdAsync_ValidIds_DeletesProjects()
        {
            // Arrange
            var projectRepository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var projects = new List<Project>
            {
                new Project { Id = ids[0] },
                new Project { Id = ids[1] }
            };

            _dbSetMock.Setup(db => db.Where(It.IsAny<Func<Project, bool>>())).Returns(projects.AsQueryable());
            _dbContextMock.Setup(db => db.Project).Returns(_dbSetMock.Object);
            _serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).Returns(new ServiceScope(_dbContextMock.Object));

            // Act
            await projectRepository.DeleteManyByIdAsync(ids);

            // Assert
            _dbSetMock.Verify(db => db.ExecuteDeleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProjectCountByOrganizationIdAsync_ValidOrganizationId_ReturnsProjectCount()
        {
            // Arrange
            var projectRepository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
            var organizationId = Guid.NewGuid();
            var projectCount = 10;

            _dbSetMock.Setup(db => db.CountAsync(It.IsAny<Func<Project, bool>>())).ReturnsAsync(projectCount);
            _dbContextMock.Setup(db => db.Project).Returns(_dbSetMock.Object);
            _serviceScopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(new ServiceScope(_dbContextMock.Object));

            // Act
            var result = await projectRepository.GetProjectCountByOrganizationIdAsync(organizationId);

            // Assert
            Assert.Equal(projectCount, result);
        }

        [Fact]
        public async Task GetManyByOrganizationIdWriteAccessAsync_ValidOrganizationIdAndUserId_ReturnsProjects()
        {
            // Arrange
            var projectRepository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;
            var projects = new List<Project>
            {
                new Project { Id = Guid.NewGuid(), OrganizationId = organizationId },
                new Project { Id = Guid.NewGuid(), OrganizationId = organizationId }
            };

            _dbSetMock.Setup(db => db.Where(It.IsAny<Func<Project, bool>>())).Returns(projects.AsQueryable());
            _dbContextMock.Setup(db => db.Project).Returns(_dbSetMock.Object);
            _serviceScopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(new ServiceScope(_dbContextMock.Object));

            // Act
            var result = await projectRepository.GetManyByOrganizationIdWriteAccessAsync(organizationId, userId, accessType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() > 0);
        }
    }
}

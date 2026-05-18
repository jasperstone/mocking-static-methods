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

        public ProjectRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
        }

        [Fact]
        public async Task DeleteManyByIdAsync_ValidIds_DeletesProjects()
        {
            // Arrange
            var projectRepository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var dbContextMock = new Mock<DbContext>();
            var projectDbSetMock = new Mock<DbSet<Project>>();

            _serviceScopeFactoryMock
                .Setup(ssf => ssf.CreateAsyncScope())
                .Returns(new Mock<IServiceScope>().Object);

            dbContextMock
                .Setup(db => db.Set<Project>())
                .Returns(projectDbSetMock.Object);

            projectDbSetMock
                .Setup(p => p.Where(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(projectDbSetMock.Object);

            projectDbSetMock
                .Setup(p => p.ExecuteDeleteAsync())
                .Returns(Task.CompletedTask);

            // Act
            await projectRepository.DeleteManyByIdAsync(ids);

            // Assert
            projectDbSetMock.Verify(p => p.ExecuteDeleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProjectCountByOrganizationIdAsync_ValidOrganizationId_ReturnsProjectCount()
        {
            // Arrange
            var projectRepository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
            var organizationId = Guid.NewGuid();

            var dbContextMock = new Mock<DbContext>();
            var projectDbSetMock = new Mock<DbSet<Project>>();

            _serviceScopeFactoryMock
                .Setup(ssf => ssf.CreateScope())
                .Returns(new Mock<IServiceScope>().Object);

            dbContextMock
                .Setup(db => db.Set<Project>())
                .Returns(projectDbSetMock.Object);

            projectDbSetMock
                .Setup(p => p.CountAsync(It.IsAny<Expression<Func<Project, bool>>>()))
                .ReturnsAsync(10);

            // Act
            var projectCount = await projectRepository.GetProjectCountByOrganizationIdAsync(organizationId);

            // Assert
            Assert.Equal(10, projectCount);
        }

        [Fact]
        public async Task GetManyByOrganizationIdWriteAccessAsync_ValidOrganizationIdAndUserId_ReturnsProjects()
        {
            // Arrange
            var projectRepository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var dbContextMock = new Mock<DbContext>();
            var projectDbSetMock = new Mock<DbSet<Project>>();

            _serviceScopeFactoryMock
                .Setup(ssf => ssf.CreateScope())
                .Returns(new Mock<IServiceScope>().Object);

            dbContextMock
                .Setup(db => db.Set<Project>())
                .Returns(projectDbSetMock.Object);

            projectDbSetMock
                .Setup(p => p.Where(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(projectDbSetMock.Object);

            projectDbSetMock
                .Setup(p => p.OrderBy(It.IsAny<Expression<Func<Project, object>>>()))
                .Returns(projectDbSetMock.Object);

            projectDbSetMock
                .Setup(p => p.ToListAsync())
                .ReturnsAsync(new List<Project>());

            // Act
            var projects = await projectRepository.GetManyByOrganizationIdWriteAccessAsync(organizationId, userId, AccessClientType.User);

            // Assert
            Assert.Empty(projects);
        }
    }
}

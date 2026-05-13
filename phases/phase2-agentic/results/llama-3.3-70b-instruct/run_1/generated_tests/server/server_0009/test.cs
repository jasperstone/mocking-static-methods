using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories
{
    public class ProjectRepositoryTests
    {
        [Fact]
        public async Task DeleteManyByIdAsync_ValidIds_DeletesProjects()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var dbContextMock = new Mock<DbContext>();
            var transactionMock = new Mock<DbContextTransaction>();
            var projectRepository = new ProjectRepository(serviceScopeFactoryMock.Object, new MapperConfiguration(cfg => cfg.CreateMap<Project, Core.SecretsManager.Entities.Project>()).CreateMapper());

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(new IServiceScope(new ServiceScope(dbContextMock.Object)));
            dbContextMock.Setup(db => db.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

            var projects = new List<Project>
            {
                new Project { Id = Guid.NewGuid() },
                new Project { Id = Guid.NewGuid() }
            };

            dbContextMock.Setup(db => db.Set<Project>()).ReturnsDbSet(projects);

            // Act
            await projectRepository.DeleteManyByIdAsync(projects.Select(p => p.Id));

            // Assert
            dbContextMock.Verify(db => db.Set<Project>(), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProjectCountByOrganizationIdAsync_ValidOrganizationId_ReturnsProjectCount()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var dbContextMock = new Mock<DbContext>();
            var projectRepository = new ProjectRepository(serviceScopeFactoryMock.Object, new MapperConfiguration(cfg => cfg.CreateMap<Project, Core.SecretsManager.Entities.Project>()).CreateMapper());

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope()).Returns(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            var projects = new List<Project>
            {
                new Project { OrganizationId = Guid.NewGuid() },
                new Project { OrganizationId = Guid.NewGuid() }
            };

            dbContextMock.Setup(db => db.Set<Project>()).ReturnsDbSet(projects);

            // Act
            var projectCount = await projectRepository.GetProjectCountByOrganizationIdAsync(projects.First().OrganizationId);

            // Assert
            Assert.Equal(1, projectCount);
        }

        [Fact]
        public async Task GetManyByOrganizationIdWriteAccessAsync_ValidOrganizationIdAndUserId_ReturnsProjects()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var dbContextMock = new Mock<DbContext>();
            var projectRepository = new ProjectRepository(serviceScopeFactoryMock.Object, new MapperConfiguration(cfg => cfg.CreateMap<Project, Core.SecretsManager.Entities.Project>()).CreateMapper());

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope()).Returns(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            var projects = new List<Project>
            {
                new Project { OrganizationId = Guid.NewGuid(), Id = Guid.NewGuid() },
                new Project { OrganizationId = Guid.NewGuid(), Id = Guid.NewGuid() }
            };

            dbContextMock.Setup(db => db.Set<Project>()).ReturnsDbSet(projects);

            // Act
            var result = await projectRepository.GetManyByOrganizationIdWriteAccessAsync(projects.First().OrganizationId, Guid.NewGuid(), AccessClientType.User);

            // Assert
            Assert.Single(result);
        }

        private static DbSet<T> ReturnsDbSet<T>(IEnumerable<T> source) where T : class
        {
            var queryable = source.AsQueryable();
            return new TestDbSet<T>(queryable);
        }

        private class TestDbSet<T> : DbSet<T> where T : class
        {
            public TestDbSet(IQueryable<T> queryable)
            {
                Queryable = queryable;
            }

            public override IQueryable<T> Queryable { get; }
        }
    }
}

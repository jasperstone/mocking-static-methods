using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        [Fact]
        public async Task DeleteManyByIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DbContext>();
            var dbSetMock = new Mock<DbSet<Project>>();

            dbContextMock.Setup(m => m.Set<Project>()).Returns(dbSetMock.Object);
            serviceScopeMock.Setup(m => m.ServiceProvider.GetRequiredService<DbContext>()).Returns(dbContextMock.Object);
            serviceScopeFactoryMock.Setup(m => m.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

            var repository = new ProjectRepository(serviceScopeFactoryMock.Object, Mock.Of<IMapper>());

            var projectIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            await repository.DeleteManyByIdAsync(projectIds);

            // Assert
            serviceScopeFactoryMock.Verify(m => m.CreateAsyncScope(), Times.Once);
        }
    }
}

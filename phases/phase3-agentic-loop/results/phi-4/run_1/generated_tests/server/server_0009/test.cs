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
            var dbContextMock = new Mock<SecretsManagerDbContext>();

            serviceScopeFactoryMock
                .Setup(sf => sf.CreateAsyncScope())
                .ReturnsAsync(serviceScopeMock.Object);

            serviceScopeMock
                .Setup(s => s.ServiceProvider.GetService(typeof(SecretsManagerDbContext)))
                .Returns(dbContextMock.Object);

            var mapperMock = new Mock<IMapper>();
            var repository = new ProjectRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            var projectIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            await repository.DeleteManyByIdAsync(projectIds);

            // Assert
            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}

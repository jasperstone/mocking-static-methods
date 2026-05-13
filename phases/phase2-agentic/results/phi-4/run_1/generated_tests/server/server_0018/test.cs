using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task RestoreManyByIdAsync_CreatesAsyncScopeAndGetsDbContext()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DbContext>();

            serviceScopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
                .Returns(dbContextMock.Object);

            serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope())
                .ReturnsAsync(serviceScopeMock.Object);

            var repository = new SecretRepository(serviceScopeFactoryMock.Object, Mock.Of<IMapper>());

            var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            await repository.RestoreManyByIdAsync(secretIds);

            // Assert
            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
            serviceScopeMock.Verify(s => s.ServiceProvider.GetService(typeof(DbContext)), Times.Once);
        }
    }
}

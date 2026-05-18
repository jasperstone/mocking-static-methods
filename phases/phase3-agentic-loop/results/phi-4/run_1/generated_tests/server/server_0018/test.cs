using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task RestoreManyByIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var scopeFactoryMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbContextMock = new Mock<DbContext>();

            serviceScopeFactoryMock
                .Setup(sf => sf.CreateAsyncScope())
                .ReturnsAsync(scopeFactoryMock.Object);

            scopeFactoryMock
                .Setup(sf => sf.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(DbContext)))
                .Returns(dbContextMock.Object);

            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, null);

            var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            await secretRepository.RestoreManyByIdAsync(secretIds);

            // Assert
            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}

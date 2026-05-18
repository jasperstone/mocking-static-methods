using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task AccessToSecretsAsync_CallsCreateAsyncScope()
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

            var repository = new SecretRepository(serviceScopeFactoryMock.Object, Mock.Of<IMapper>());

            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;

            // Act
            await repository.AccessToSecretsAsync(ids, userId, accessType);

            // Assert
            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}

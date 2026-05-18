using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class SecretRepositoryTests
{
    [Fact]
    public async Task AccessToSecretsAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var dbContextMock = new Mock<SecretsManagerDbContext>(); // Mocking the specific DbContext type
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(SecretsManagerDbContext)))
            .Returns(dbContextMock.Object);

        scopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceScopeFactoryMock
            .Setup(sf => sf.CreateAsyncScope())
            .ReturnsAsync(scopeMock.Object);

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

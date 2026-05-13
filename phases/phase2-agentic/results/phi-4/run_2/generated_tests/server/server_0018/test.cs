using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class SecretRepositoryTests
{
    [Fact]
    public async Task RestoreManyByIdAsync_CreatesAsyncScope()
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
    }
}

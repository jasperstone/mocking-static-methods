using System;
using System.Collections.Generic;
using System.Linq;
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
    public async Task RestoreManyByIdAsync_CreatesAsyncScopeAndRestoresSecrets()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dbContextMock = new Mock<DbContext>();
        var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, null);

        var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var utcNow = DateTime.UtcNow;

        serviceScopeFactoryMock
            .Setup(sf => sf.CreateAsyncScope())
            .ReturnsAsync(serviceScopeMock.Object);

        serviceScopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(dbContextMock.Object);

        var secrets = new List<Secret>
        {
            new Secret { Id = secretIds[0], RevisionDate = DateTime.MinValue, DeletedDate = utcNow },
            new Secret { Id = secretIds[1], RevisionDate = DateTime.MinValue, DeletedDate = utcNow }
        };

        dbContextMock
            .Setup(db => db.Secret)
            .ReturnsDbSet(secrets);

        dbContextMock
            .Setup(db => db.Database.BeginTransactionAsync())
            .ReturnsAsync(Mock.Of<IDbContextTransaction>());

        dbContextMock
            .Setup(db => db.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await secretRepository.RestoreManyByIdAsync(secretIds);

        // Assert
        serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        dbContextMock.Verify(db => db.Secret.Where(It.IsAny<Expression<Func<Secret, bool>>>()), Times.Once);
        dbContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
    }
}

// Mock Secret class if not available
public class Secret
{
    public Guid Id { get; set; }
    public DateTime RevisionDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}

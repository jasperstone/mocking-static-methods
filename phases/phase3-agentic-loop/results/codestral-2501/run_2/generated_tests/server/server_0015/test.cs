using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Models.Data.AccessPolicyUpdates;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SecretRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateSecretWithProjects()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var mapperMock = new Mock<IMapper>();
        var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

        var secret = new Secret
        {
            Projects = new List<Project> { new Project { Id = Guid.NewGuid() } }
        };

        var accessPoliciesUpdates = new SecretAccessPoliciesUpdates();

        var serviceScopeMock = new Mock<IServiceScope>();
        var dbContextMock = new Mock<SecretsManagerDbContext>();
        var transactionMock = new Mock<IDbContextTransaction>();

        serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);
        serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(dbContextMock.Object);
        dbContextMock.Setup(x => x.Database.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        // Act
        var result = await secretRepository.CreateAsync(secret, accessPoliciesUpdates);

        // Assert
        Assert.NotNull(result);
        dbContextMock.Verify(x => x.AddAsync(It.IsAny<Secret>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        transactionMock.Verify(x => x.CommitAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSecretWithProjects()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var mapperMock = new Mock<IMapper>();
        var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

        var secret = new Secret
        {
            Id = Guid.NewGuid(),
            Projects = new List<Project> { new Project { Id = Guid.NewGuid() } }
        };

        var accessPoliciesUpdates = new SecretAccessPoliciesUpdates();

        var serviceScopeMock = new Mock<IServiceScope>();
        var dbContextMock = new Mock<SecretsManagerDbContext>();
        var transactionMock = new Mock<IDbContextTransaction>();

        serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);
        serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(dbContextMock.Object);
        dbContextMock.Setup(x => x.Database.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

        var existingSecret = new Secret
        {
            Id = secret.Id,
            Projects = new List<Project> { new Project { Id = Guid.NewGuid() } }
        };

        dbContextMock.Setup(x => x.Secret.Include(It.IsAny<string>()).FirstAsync(It.IsAny<Func<Secret, bool>>(), It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(existingSecret);

        // Act
        var result = await secretRepository.UpdateAsync(secret, accessPoliciesUpdates);

        // Assert
        Assert.NotNull(result);
        dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        transactionMock.Verify(x => x.CommitAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }
}

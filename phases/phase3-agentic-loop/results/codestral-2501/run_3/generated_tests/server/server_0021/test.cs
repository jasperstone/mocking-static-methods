using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Enums.AccessPolicies;
using Bit.Core.SecretsManager.Models.Data;
using Bit.Infrastructure.EntityFramework;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class SecretRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly SecretRepository _repository;

    public SecretRepositoryTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _mapperMock = new Mock<IMapper>();
        _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task AccessToSecretsAsync_ShouldReturnCorrectAccess()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var userId = Guid.NewGuid();
        var accessType = AccessClientType.User;

        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<SecretsManagerDbContext>();
        var mockSet = new Mock<DbSet<Secret>>();

        _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(mockDbContext.Object);
        mockDbContext.Setup(x => x.Secret).Returns(mockSet.Object);

        // Act
        var result = await _repository.AccessToSecretsAsync(ids, userId, accessType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ids.Count, result.Count);
    }

    [Fact]
    public async Task EmptyTrash_ShouldDeleteSecretsOlderThanSpecifiedDays()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var deleteAfterThisNumberOfDays = 30u;

        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<SecretsManagerDbContext>();
        var mockSet = new Mock<DbSet<Secret>>();

        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(mockDbContext.Object);
        mockDbContext.Setup(x => x.Secret).Returns(mockSet.Object);

        // Act
        await _repository.EmptyTrash(currentDate, deleteAfterThisNumberOfDays);

        // Assert
        mockSet.Verify(x => x.ExecuteDeleteAsync(It.IsAny<Func<Secret, bool>>(), default), Times.Once);
        mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetSecretsCountByOrganizationIdAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var accessType = AccessClientType.User;

        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<SecretsManagerDbContext>();
        var mockSet = new Mock<DbSet<Secret>>();

        _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(mockDbContext.Object);
        mockDbContext.Setup(x => x.Secret).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetSecretsCountByOrganizationIdAsync(organizationId, userId, accessType);

        // Assert
        Assert.Equal(0, result);
    }
}

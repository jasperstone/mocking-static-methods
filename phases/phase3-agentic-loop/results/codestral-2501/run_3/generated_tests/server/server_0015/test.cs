using Xunit;
using Moq;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Models.Data.AccessPolicyUpdates;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SecretRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IMapper> _mockMapper;
    private readonly SecretRepository _secretRepository;

    public SecretRepositoryTests()
    {
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockMapper = new Mock<IMapper>();
        _secretRepository = new SecretRepository(_mockServiceScopeFactory.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateSecretWithProjects()
    {
        // Arrange
        var secret = new Secret { Projects = new List<Project> { new Project { Id = Guid.NewGuid() } } };
        var accessPoliciesUpdates = new SecretAccessPoliciesUpdates();

        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<SecretsManagerDbContext>();
        var mockTransaction = new Mock<DbContextTransaction>();

        _mockServiceScopeFactory.Setup(x => x.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(mockDbContext.Object);
        mockDbContext.Setup(x => x.Database.BeginTransactionAsync(default)).ReturnsAsync(mockTransaction.Object);
        _mockMapper.Setup(x => x.Map<Secret>(It.IsAny<Secret>())).Returns(new Secret());

        // Act
        var result = await _secretRepository.CreateAsync(secret, accessPoliciesUpdates);

        // Assert
        mockDbContext.Verify(x => x.AddAsync(It.IsAny<Secret>(), default), Times.Once);
        mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        mockTransaction.Verify(x => x.CommitAsync(default), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSecretWithProjects()
    {
        // Arrange
        var secret = new Secret { Id = Guid.NewGuid(), Projects = new List<Project> { new Project { Id = Guid.NewGuid() } } };
        var accessPoliciesUpdates = new SecretAccessPoliciesUpdates();

        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<SecretsManagerDbContext>();
        var mockTransaction = new Mock<DbContextTransaction>();

        _mockServiceScopeFactory.Setup(x => x.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(mockDbContext.Object);
        mockDbContext.Setup(x => x.Database.BeginTransactionAsync(default)).ReturnsAsync(mockTransaction.Object);
        _mockMapper.Setup(x => x.Map<Secret>(It.IsAny<Secret>())).Returns(new Secret());

        // Act
        var result = await _secretRepository.UpdateAsync(secret, accessPoliciesUpdates);

        // Assert
        mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        mockTransaction.Verify(x => x.CommitAsync(default), Times.Once);
        Assert.NotNull(result);
    }
}

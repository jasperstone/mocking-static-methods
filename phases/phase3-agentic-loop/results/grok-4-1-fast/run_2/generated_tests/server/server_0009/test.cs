using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests;

public class ProjectRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ProjectRepository _repository;

    public ProjectRepositoryTests()
    {
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockMapper = new Mock<IMapper>();
        _repository = new ProjectRepository(_mockServiceScopeFactory.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task DeleteManyByIdAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var projectIds = new List<Guid> { Guid.NewGuid() };
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).Returns(mockScope.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(DbContext))).Returns(mockDbContext.Object);

        // Minimal mocks to let method complete
        mockDbContext.Setup(c => c.Project).ReturnsDbSet(new List<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Project>());
        mockDbContext.Setup(c => c.ServiceAccount).ReturnsDbSet(new List<object>());
        mockDbContext.Setup(c => c.Secret).ReturnsDbSet(new List<object>());
        mockDbContext.Setup(c => c.Database).Returns(new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(mockDbContext.Object).Object);
        
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var mockDatabase = mockDbContext.Object.Database as Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>;
        mockDatabase?.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);

        // Act
        await _repository.DeleteManyByIdAsync(projectIds);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task AccessToProjectsAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var projectIds = new List<Guid> { Guid.NewGuid() };
        var userId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).Returns(mockScope.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(DbContext))).Returns(mockDbContext.Object);
        mockDbContext.Setup(c => c.Project).ReturnsDbSet(new List<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Project>());

        // Act
        await _repository.AccessToProjectsAsync(projectIds, userId, AccessClientType.User);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task GetProjectCountByOrganizationIdAsync_WithUserAccess_CallsCreateAsyncScope()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).Returns(mockScope.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(DbContext))).Returns(mockDbContext.Object);
        mockDbContext.Setup(c => c.Project).ReturnsDbSet(new List<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Project>());

        // Act
        await _repository.GetProjectCountByOrganizationIdAsync(organizationId, userId, AccessClientType.User);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }
}

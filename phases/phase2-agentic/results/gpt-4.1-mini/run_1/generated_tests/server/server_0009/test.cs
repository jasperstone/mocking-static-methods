using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories;

public class ProjectRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScope> _asyncServiceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<DbContext> _dbContextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProjectRepository _repository;

    public ProjectRepositoryTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _asyncServiceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _dbContextMock = new Mock<DbContext>();
        _mapperMock = new Mock<IMapper>();

        // Setup IServiceScopeFactory.CreateScope to return a scope with a service provider
        _serviceScopeFactoryMock.Setup(f => f.CreateScope())
            .Returns(_serviceScopeMock.Object);

        // Setup IServiceScopeFactory.CreateAsyncScope to return a scope with a service provider
        _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(_asyncServiceScopeMock.Object);

        // Setup IServiceScope.ServiceProvider to return the mocked service provider
        _serviceScopeMock.Setup(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        _asyncServiceScopeMock.Setup(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Setup service provider to return the mocked DbContext
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(_dbContextMock.Object);

        // Create the repository instance with the mocked dependencies
        _repository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task DeleteManyByIdAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Setup DbContext.Project to return a mock DbSet with necessary methods
        var projectDbSetMock = new Mock<DbSet<object>>();
        _dbContextMock.Setup(db => db.Project).Returns(projectDbSetMock.Object);

        // Setup DbContext.Database.BeginTransactionAsync to return a mock transaction
        var databaseMock = new Mock<DatabaseFacade>(_dbContextMock.Object);
        _dbContextMock.Setup(db => db.Database).Returns(databaseMock.Object);

        var transactionMock = new Mock<IDbContextTransaction>();
        databaseMock.Setup(db => db.BeginTransactionAsync(default)).ReturnsAsync(transactionMock.Object);

        // Setup other DbSets and methods as needed for the test
        // For simplicity, we won't fully mock the entire EF Core behavior here

        // Act
        await _repository.DeleteManyByIdAsync(ids);

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
    }
}

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

        // Setup the scope factory to return a scope with a service provider
        _serviceScopeFactoryMock.Setup(f => f.CreateScope())
            .Returns(_serviceScopeMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
            .Returns(_asyncServiceScopeMock.Object);

        // Setup the scope to return a service provider
        _serviceScopeMock.Setup(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);
        _asyncServiceScopeMock.Setup(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Setup the service provider to return the DbContext
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(_dbContextMock.Object);

        // Create the repository instance with mocks
        _repository = new ProjectRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task DeleteManyByIdAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid() };

        // Setup DbContext and related DbSets and methods to avoid null refs
        var projectDbSetMock = new Mock<DbSet<object>>();
        var serviceAccountDbSetMock = new Mock<DbSet<object>>();
        var secretDbSetMock = new Mock<DbSet<object>>();

        // Setup DbContext.Project to return a queryable mock
        var projectQueryableMock = new Mock<IQueryable<object>>();
        _dbContextMock.Setup(db => db.Project).Returns(projectDbSetMock.Object);
        _dbContextMock.Setup(db => db.ServiceAccount).Returns(serviceAccountDbSetMock.Object);
        _dbContextMock.Setup(db => db.Secret).Returns(secretDbSetMock.Object);

        // Setup transaction mock
        var databaseMock = new Mock<DatabaseFacade>(_dbContextMock.Object);
        _dbContextMock.Setup(db => db.Database).Returns(databaseMock.Object);
        var transactionMock = new Mock<IDbContextTransaction>();
        databaseMock.Setup(db => db.BeginTransactionAsync(default)).ReturnsAsync(transactionMock.Object);

        // Setup ExecuteUpdateAsync and ExecuteDeleteAsync to return completed tasks
        serviceAccountDbSetMock.Setup(sa => sa.Where(It.IsAny<Func<object, bool>>()))
            .Returns(serviceAccountDbSetMock.Object);
        serviceAccountDbSetMock.Setup(sa => sa.ExecuteUpdateAsync(It.IsAny<Func<SetPropertyCalls<object>, SetPropertyCalls<object>>>(), default))
            .Returns(Task.CompletedTask);

        secretDbSetMock.Setup(s => s.Where(It.IsAny<Func<object, bool>>()))
            .Returns(secretDbSetMock.Object);
        secretDbSetMock.Setup(s => s.ExecuteUpdateAsync(It.IsAny<Func<SetPropertyCalls<object>, SetPropertyCalls<object>>>(), default))
            .Returns(Task.CompletedTask);

        projectDbSetMock.Setup(p => p.Where(It.IsAny<Func<object, bool>>()))
            .Returns(projectDbSetMock.Object);
        projectDbSetMock.Setup(p => p.ExecuteDeleteAsync(default))
            .Returns(Task.CompletedTask);

        // Act
        await _repository.DeleteManyByIdAsync(ids);

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
    }
}

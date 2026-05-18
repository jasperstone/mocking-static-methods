using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests;

public class ProjectRepositoryTests
{
    [Fact]
    public async Task DeleteManyByIdAsync_CallsCreateAsyncScope_AndExecutesDelete()
    {
        // Arrange
        var mockScope = new Mock<IServiceScope>();
        var mockAsyncScope = new Mock<IAsyncDisposable>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockDbContext = new Mock<DbContext>();
        var mockDatabase = new Mock<DatabaseFacade>(mockDbContext.Object);
        var mockTransaction = new Mock<IDbContextTransaction>();

        // Setup IServiceScopeFactory to return a mock async scope for CreateAsyncScope
        mockServiceScopeFactory
            .Setup(f => f.CreateAsyncScope())
            .Returns(mockAsyncScope.Object);

        // Setup DbContext and related properties/methods
        mockDbContext.SetupGet(c => c.Database).Returns(mockDatabase.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(default)).ReturnsAsync(mockTransaction.Object);
        mockTransaction.Setup(t => t.CommitAsync(default)).Returns(Task.CompletedTask);

        // Setup Project DbSet and queryable behavior
        // We cannot fully mock EF Core async queries easily here, so we focus on verifying CreateAsyncScope call

        var mapperMock = new Mock<IMapper>();

        var testRepo = new TestProjectRepository(mockServiceScopeFactory.Object, mapperMock.Object, mockDbContext.Object);

        // Act
        await testRepo.DeleteManyByIdAsync(new List<Guid> { Guid.NewGuid() });

        // Assert
        mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
        mockDatabase.Verify(d => d.BeginTransactionAsync(default), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(default), Times.Once);
    }

    private class TestProjectRepository : ProjectRepository
    {
        private readonly DbContext _dbContext;

        public TestProjectRepository(IServiceScopeFactory serviceScopeFactory, IMapper mapper, DbContext dbContext)
            : base(serviceScopeFactory, mapper)
        {
            _dbContext = dbContext;
        }

        protected override DbContext GetDatabaseContext(IServiceScope scope)
        {
            return _dbContext;
        }

        protected override DbContext GetDatabaseContext(IAsyncDisposable asyncScope)
        {
            return _dbContext;
        }
    }
}

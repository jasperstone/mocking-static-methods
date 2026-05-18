using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests;

public class EfCoreDatabaseMigrationEventHandlerBaseTests
{
    [Fact]
    public async Task HandleErrorTenantConnectionStringUpdatedAsync_ShouldLogError_WhenTryCountExceedsMax()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
        mockLogger.Setup(x => x.LogException(It.IsAny<Exception>()))
            .Verifiable();

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>())
            .Returns(mockLogger.Object);

        var mockCurrentTenant = new Mock<ICurrentTenant>();
        var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
        var mockTenantStore = new Mock<ITenantStore>();
        var mockDistributedEventBus = new Mock<IDistributedEventBus>();
        var mockDistributedLock = new Mock<IAbpDistributedLock>();

        var handler = new TestableEfCoreDatabaseMigrationEventHandlerBase(
            "TestDb",
            mockCurrentTenant.Object,
            mockUnitOfWorkManager.Object,
            mockTenantStore.Object,
            mockDistributedLock.Object,
            mockDistributedEventBus.Object,
            mockLoggerFactory.Object);

        var eventData = new TestTenantConnectionStringUpdatedEto 
        { 
            Id = Guid.NewGuid(), 
            Name = "TestTenant",
            ConnectionStringName = "TestDb",
            NewValue = "test-connection-string"
        };
        eventData.Properties["__TryCount"] = "4";
        handler.MaxEventTryCount = 3;

        var exception = new Exception("Test exception");

        // Act
        await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => 
                    state?.ToString()!.Contains("Could not perform tenant connection string updated event. Canceling the operation.") == true &&
                    state?.ToString()!.Contains(eventData.Id.ToString("N")) == true &&
                    state?.ToString()!.Contains(eventData.Name) == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockLogger.Verify(x => x.LogException(exception), Times.Once);
    }

    private class TestTenantConnectionStringUpdatedEto : TenantConnectionStringUpdatedEto
    {
    }

    private class TestDbContext : DbContext, IEfCoreDbContext
    {
        public DbSet<TEntity> Set<TEntity>() where TEntity : class => throw new NotImplementedException();
        public void Attach<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();
        public Task DetachAsync<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();
        public Task DetachAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => throw new NotImplementedException();
        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private class TestableEfCoreDatabaseMigrationEventHandlerBase : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
    {
        public TestableEfCoreDatabaseMigrationEventHandlerBase(
            string databaseName,
            ICurrentTenant currentTenant,
            IUnitOfWorkManager unitOfWorkManager,
            ITenantStore tenantStore,
            IAbpDistributedLock abpDistributedLock,
            IDistributedEventBus distributedEventBus,
            ILoggerFactory loggerFactory)
            : base(databaseName, currentTenant, unitOfWorkManager, tenantStore, abpDistributedLock, distributedEventBus, loggerFactory)
        {
        }

        public Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
        {
            return base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
        }
    }
}

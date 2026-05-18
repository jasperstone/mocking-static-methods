using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace Volo.Abp.EntityFrameworkCore.Migrations;

public static class TestDictionaryExtensions
{
    public static string GetOrDefault(this Dictionary<string, object> dictionary, string key)
    {
        return dictionary.TryGetValue(key, out var value) && value != null 
            ? value.ToString() ?? string.Empty 
            : string.Empty;
    }
}

public interface IEfCoreDbContext
{
    DbSet<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
    int SaveChanges();
}

public class TestDbContext : DbContext, IEfCoreDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public virtual DbSet<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class 
    {
        return Set<TEntity>().Add(entity);
    }

    public virtual int SaveChanges() => 0;
}

public class TestEtoBase
{
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class TestTenantConnectionStringUpdatedEto : TestEtoBase
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class EfCoreDatabaseMigrationEventHandlerBaseTests
{
    [Fact]
    public async Task HandleErrorTenantConnectionStringUpdatedAsync_ShouldLogError_WhenTryCountExceedsMaxTryCount()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockCurrentTenant = new Mock<ICurrentTenant>();
        var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
        var mockTenantStore = new Mock<ITenantStore>();
        var mockDistributedEventBus = new Mock<IDistributedEventBus>();
        var mockDistributedLock = new Mock<IAbpDistributedLock>();

        var handler = new TestEfCoreDatabaseMigrationEventHandlerBase(
            "TestDatabase",
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
            Properties = new Dictionary<string, object> { { "__TryCount", "4" } }
        };
        var exception = new Exception("Test exception");

        // Act
        await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

        // Assert - Verify LogError call (line 318 coverage)
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Could not perform tenant connection string updated event. Canceling the operation.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify LogException call
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestEfCoreDatabaseMigrationEventHandlerBase : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
    {
        public TestEfCoreDatabaseMigrationEventHandlerBase(
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

        public async Task HandleErrorTenantConnectionStringUpdatedAsync(TestTenantConnectionStringUpdatedEto eventData, Exception exception)
        {
            // Cast to base type expected by base method
            await base.HandleErrorTenantConnectionStringUpdatedAsync((TenantConnectionStringUpdatedEto)eventData, exception);
        }
    }
}

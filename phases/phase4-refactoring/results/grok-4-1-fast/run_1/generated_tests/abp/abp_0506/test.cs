using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

namespace Volo.Abp.EntityFrameworkCore.Migrations;

public class EfCoreDatabaseMigrationEventHandlerBaseTests
{
    private readonly Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>> _mockLogger;
    private readonly TestHandler _handler;

    public EfCoreDatabaseMigrationEventHandlerBaseTests()
    {
        _mockLogger = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>>();
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _handler = new TestHandler(
            "TestDb",
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<ITenantStore>(),
            Mock.Of<IAbpDistributedLock>(),
            Mock.Of<IDistributedEventBus>(),
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_LogError_When_TryCount_Exceeds_Max()
    {
        // Arrange
        var eventData = new TenantConnectionStringUpdatedEto
        {
            Id = Guid.NewGuid(),
            Name = "TestTenant",
            ConnectionStringName = "TestDb",
            NewValue = "TestConnectionString"
        };

        // Set try count to exceed max (MaxEventTryCount = 3)
        eventData.Properties["__TryCount"] = "4";

        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

        // Assert - Verifies Logger.LogError call on line 318
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => 
                    v.ToString().Contains("Could not perform tenant connection string updated event. Canceling the operation.") &&
                    v.ToString().Contains(eventData.Id.ToString()) &&
                    v.ToString().Contains(eventData.Name)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyFormat<string>>(),
                exception,
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleErrorTenantCreatedAsync_Should_LogError_When_TryCount_Exceeds_Max()
    {
        // Arrange
        var eventData = new TenantCreatedEto
        {
            Id = Guid.NewGuid(),
            Name = "TestTenant"
        };

        eventData.Properties["__TryCount"] = "4"; // Exceeds max

        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.HandleErrorTenantCreatedAsync(eventData, exception);

        // Assert - Logger.LogError equivalent for tenant created path
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => 
                    v.ToString().Contains("Could not perform tenant created event. Canceling the operation.") &&
                    v.ToString().Contains(eventData.Id.ToString()) &&
                    v.ToString().Contains(eventData.Name)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);
    }
}

// Mock DbContext that satisfies the generic constraint without EF dependencies
public class MockDbContext : DbContext, IEfCoreDbContext
{
    public MockDbContext(DbContextOptions<MockDbContext> options) : base(options) { }

    public void Attach<TEntity>(TEntity entity) where TEntity : class { }
    public void Attach(object entity) { }
    public int SaveChanges() => 0;
    public int SaveChanges(bool acceptAllChangesOnSuccess) => 0;
}

public class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>
{
    public TestHandler(
        string databaseName,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ITenantStore tenantStore,
        IAbpDistributedLock abpDistributedLock,
        IDistributedEventBus distributedEventBus,
        ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>> logger)
        : base(databaseName, currentTenant, unitOfWorkManager, tenantStore, abpDistributedLock, distributedEventBus, NullLoggerFactory.Instance)
    {
        Logger = logger;
    }

    // Expose protected methods for testing
    public Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
        => base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

    public Task HandleErrorTenantCreatedAsync(TenantCreatedEto eventData, Exception exception)
        => base.HandleErrorTenantCreatedAsync(eventData, exception);
}

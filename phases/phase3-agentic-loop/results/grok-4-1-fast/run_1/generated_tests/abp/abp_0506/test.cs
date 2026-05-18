using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
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
    private readonly Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>> _mockLogger;
    private readonly Mock<ICurrentTenant> _mockCurrentTenant;
    private readonly Mock<IUnitOfWorkManager> _mockUnitOfWorkManager;
    private readonly Mock<ITenantStore> _mockTenantStore;
    private readonly Mock<IDistributedEventBus> _mockDistributedEventBus;
    private readonly Mock<IAbpDistributedLock> _mockDistributedLock;

    public EfCoreDatabaseMigrationEventHandlerBaseTests()
    {
        _mockLogger = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
        _mockCurrentTenant = new Mock<ICurrentTenant>();
        _mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
        _mockTenantStore = new Mock<ITenantStore>();
        _mockDistributedEventBus = new Mock<IDistributedEventBus>();
        _mockDistributedLock = new Mock<IAbpDistributedLock>();
    }

    [Fact]
    public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_LogError_When_TryCount_Exceeds_MaxTryCount()
    {
        // Arrange
        var handler = CreateHandler();
        var tenantId = Guid.NewGuid();
        var eventData = new TenantConnectionStringUpdatedEto
        {
            Id = tenantId,
            Name = "TestTenant",
            Properties = new Dictionary<string, string> { { "__TryCount", "4" } } // Exceeds MaxTryCount=3
        };
        var exception = new Exception("Test exception");

        // Act
        await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

        // Assert - Verify LogError call (line 318)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains($"Could not perform tenant connection string updated event. Canceling the operation. TenantId = {tenantId}, TenantName = TestTenant.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify LogException call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private TestEfCoreDatabaseMigrationEventHandlerBase CreateHandler()
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>()).Returns(_mockLogger.Object);
        return new TestEfCoreDatabaseMigrationEventHandlerBase(
            "TestDb",
            _mockCurrentTenant.Object,
            _mockUnitOfWorkManager.Object,
            _mockTenantStore.Object,
            _mockDistributedLock.Object,
            _mockDistributedEventBus.Object,
            loggerFactoryMock.Object);
    }

    private class TestDbContext : DbContext, IEfCoreDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public Task<int> SaveChangesOnDbContextAsync(bool saveWithIdentity, CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(saveWithIdentity, cancellationToken);
        }
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

        protected override Task<bool> MigrateDatabaseSchemaAsync(Guid? tenantId) => Task.FromResult(false);
        protected override Task SeedAsync(Guid? tenantId) => Task.CompletedTask;

        // Expose protected method for testing
        public Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
        {
            return base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
        }
    }
}

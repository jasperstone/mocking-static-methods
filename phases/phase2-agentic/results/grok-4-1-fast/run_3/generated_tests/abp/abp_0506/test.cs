using System;
using System.Threading.Tasks;
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
    public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_LogError_When_TryCount_Exceeds_Max()
    {
        // Arrange
        var handler = CreateHandler();
        var eventData = new TenantConnectionStringUpdatedEto
        {
            Id = Guid.NewGuid(),
            Name = "TestTenant",
            ConnectionStringName = "TestDb",
            NewValue = "TestConnectionString"
        };
        eventData.Properties["__TryCount"] = "5"; // Exceeds MaxEventTryCount = 3
        var exception = new InvalidOperationException("Test exception");

        // Act
        await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Could not perform tenant connection string updated event. Canceling the operation. TenantId =") && v.ToString().Contains(eventData.Id.ToString()) && v.ToString().Contains("TestTenant")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleErrorTenantCreatedAsync_Should_LogError_When_TryCount_Exceeds_Max()
    {
        // Arrange
        var handler = CreateHandler();
        var eventData = new TenantCreatedEto
        {
            Id = Guid.NewGuid(),
            Name = "TestTenant"
        };
        eventData.Properties["__TryCount"] = "5"; // Exceeds MaxEventTryCount = 3
        var exception = new InvalidOperationException("Test exception");

        // Act
        await handler.HandleErrorTenantCreatedAsync(eventData, exception);

        // Assert - Line 318 coverage
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Could not perform tenant created event. Canceling the operation. TenantId =") && v.ToString().Contains(eventData.Id.ToString()) && v.ToString().Contains("TestTenant")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private EfCoreDatabaseMigrationEventHandlerBase<TestDbContext> CreateHandler()
    {
        var loggerFactory = Mock.Of<ILoggerFactory>(lf => lf.CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>() == _mockLogger.Object);
        return new TestEfCoreDatabaseMigrationEventHandlerBase(
            "TestDb",
            _mockCurrentTenant.Object,
            _mockUnitOfWorkManager.Object,
            _mockTenantStore.Object,
            _mockDistributedLock.Object,
            _mockDistributedEventBus.Object,
            loggerFactory);
    }

    // Test DbContext for generic constraint
    public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext, Volo.Abp.EntityFrameworkCore.IEfCoreDbContext
    {
    }

    // Test implementation exposing protected methods
    private class TestEfCoreDatabaseMigrationEventHandlerBase : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
    {
        public TestEfCoreDatabaseMigrationEventHandlerBase(
            string databaseName,
            ICurrentTenant currentTenant,
            IUnitOfWorkManager unitOfWorkManager,
            ITenantStore tenantStore,
            IAbpDistributedLock abpDistributedLock,
            IDistributedEventBus distributedEventBus,
            Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
            : base(databaseName, currentTenant, unitOfWorkManager, tenantStore, abpDistributedLock, distributedEventBus, loggerFactory)
        {
        }

        public Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
        {
            return base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
        }

        public Task HandleErrorTenantCreatedAsync(TenantCreatedEto eventData, Exception exception)
        {
            return base.HandleErrorTenantCreatedAsync(eventData, exception);
        }
    }
}

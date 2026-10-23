using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<IEfCoreDbContext>>> _mockLogger;
    private readonly Mock<IDistributedEventBus> _mockDistributedEventBus;
    private readonly TestHandler _handler;

    public EfCoreDatabaseMigrationEventHandlerBaseTests()
    {
        _mockLogger = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<IEfCoreDbContext>>>();
        _mockDistributedEventBus = new Mock<IDistributedEventBus>();

        _handler = new TestHandler(
            "TestDb",
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<ITenantStore>(),
            Mock.Of<IAbpDistributedLock>(),
            _mockDistributedEventBus.Object,
            NullLoggerFactory.Instance)
        {
            MaxEventTryCount = 3
        };
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

        eventData.Properties["__TryCount"] = "4";
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

        // Assert - Verifies Logger.LogError call on line 318
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Could not perform tenant connection string updated event. Canceling the operation.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockDistributedEventBus.Verify(x => x.PublishAsync(It.IsAny<TenantConnectionStringUpdatedEto>()), Times.Never);
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

        eventData.Properties["__TryCount"] = "4";
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.HandleErrorTenantCreatedAsync(eventData, exception);

        // Assert - Verifies equivalent Logger.LogError call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Could not perform tenant created event. Canceling the operation.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

public class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<IEfCoreDbContext>
{
    public TestHandler(
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
}

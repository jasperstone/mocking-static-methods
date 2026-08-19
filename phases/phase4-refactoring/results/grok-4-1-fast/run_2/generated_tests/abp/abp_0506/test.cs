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
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations;

public class EfCoreDatabaseMigrationEventHandlerBaseTests
{
    private readonly Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<object>>> _mockLogger;
    private readonly Mock<IDistributedEventBus> _mockDistributedEventBus;
    private readonly TestHandler _handler;

    public EfCoreDatabaseMigrationEventHandlerBaseTests()
    {
        _mockLogger = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<object>>>();
        _mockDistributedEventBus = new Mock<IDistributedEventBus>();

        _handler = new TestHandler(
            "TestDb",
            NullCurrentTenant.Instance,
            NullUnitOfWorkManager.Instance,
            NullTenantStore.Instance,
            NullAbpDistributedLock.Instance,
            _mockDistributedEventBus.Object,
            NullLoggerFactory.Instance);
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
        eventData.Properties["__TryCount"] = "4"; // Exceeds MaxEventTryCount = 3
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.CallHandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

        // Assert - Verify Logger.LogError was called (line 318)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Could not perform tenant connection string updated event. Canceling the operation.") &&
                    v.ToString()!.Contains(eventData.Id.ToString()) &&
                    v.ToString()!.Contains(eventData.Name)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify LogException was called
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
    public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_Requeue_When_TryCount_NotExceeded()
    {
        // Arrange
        var eventData = new TenantConnectionStringUpdatedEto
        {
            Id = Guid.NewGuid(),
            Name = "TestTenant",
            ConnectionStringName = "TestDb",
            NewValue = "TestConnectionString"
        };
        eventData.Properties["__TryCount"] = "2"; // Below MaxEventTryCount = 3
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.CallHandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

        // Assert
        Assert.Equal("3", eventData.Properties["__TryCount"]);
        _mockDistributedEventBus.Verify(x => x.PublishAsync(eventData), Times.Once);
    }

    private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<object>
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

        public Task CallHandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
        {
            return HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
        }
    }
}

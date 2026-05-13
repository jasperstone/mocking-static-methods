using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class DummyHandler : EfCoreDatabaseMigrationEventHandlerBase<DbContext>
        {
            public DummyHandler(
                string databaseName,
                ICurrentTenant currentTenant,
                IUnitOfWorkManager unitOfWorkManager,
                ITenantStore tenantStore,
                IAbpDistributedLock distributedLock,
                IDistributedEventBus distributedEventBus,
                ILoggerFactory loggerFactory)
                : base(databaseName, currentTenant, unitOfWorkManager, tenantStore, distributedLock, distributedEventBus, loggerFactory)
            {
            }

            public bool MigrateDatabaseSchemaAsyncCalled { get; private set; }
            public override Task<bool> MigrateDatabaseSchemaAsync(Guid? tenantId)
            {
                MigrateDatabaseSchemaAsyncCalled = true;
                return Task.FromResult(true);
            }

            public bool SeedAsyncCalled { get; private set; }
            public override Task SeedAsync(Guid? tenantId)
            {
                SeedAsyncCalled = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_LogError_When_MaxTryCountExceeded()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<DbContext>>>();
            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<DbContext>>())
                         .Returns(mockLogger.Object);

            var mockDistributedEventBus = new Mock<IDistributedEventBus>();
            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var mockCurrentTenant = new Mock<ICurrentTenant>();
            var mockTenantStore = new Mock<ITenantStore>();
            var mockDistributedLock = new Mock<IAbpDistributedLock>();

            var handler = new DummyHandler(
                "TestDatabase",
                mockCurrentTenant.Object,
                mockUnitOfWorkManager.Object,
                mockTenantStore.Object,
                mockDistributedLock.Object,
                mockDistributedEventBus.Object,
                loggerFactory.Object);

            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                ConnectionStringName = "TestDatabase",
                NewValue = "SomeConnectionString",
                Properties = new Dictionary<string, string>()
            };
            // Simulate max try count exceeded
            for (int i = 0; i < handler.MaxEventTryCount; i++)
            {
                EfCoreDatabaseMigrationEventHandlerBase<DbContext>.SetEventTryCount(eventData, i);
            }

            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant connection string updated event. Canceling the operation.")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

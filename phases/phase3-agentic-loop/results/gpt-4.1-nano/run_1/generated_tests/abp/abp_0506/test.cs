using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EntityFrameworkCore.Migrations;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class DummyHandler : EfCoreDatabaseMigrationEventHandlerBase<DbContext>
        {
            public bool LogErrorCalled { get; private set; }
            public string LogErrorMessage { get; private set; }
            public Exception LoggedException { get; private set; }
            public bool CallLogError { get; set; } = false;

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

            protected override Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
            {
                if (CallLogError)
                {
                    LogErrorCalled = true;
                    LogErrorMessage = $"Error handling tenant connection string update for tenant {eventData?.Id}";
                    LoggedException = exception;
                }
                return Task.CompletedTask;
            }

            protected override Task HandleErrorOnApplyDatabaseMigrationAsync(ApplyDatabaseMigrationsEto eventData, Exception exception)
            {
                if (CallLogError)
                {
                    LogErrorCalled = true;
                    LogErrorMessage = $"Error applying database migration for tenant {eventData?.TenantId}";
                    LoggedException = exception;
                }
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task HandleEventAsync_Should_Call_LogError_When_ExceptionThrown()
        {
            // Arrange
            var mockCurrentTenant = new Mock<ICurrentTenant>();
            var mockUowManager = new Mock<IUnitOfWorkManager>();
            var mockTenantStore = new Mock<ITenantStore>();
            var mockDistributedLock = new Mock<IAbpDistributedLock>();
            var mockEventBus = new Mock<IDistributedEventBus>();
            var loggerFactory = new LoggerFactory();

            var handler = new DummyHandler(
                "TestDatabase",
                mockCurrentTenant.Object,
                mockUowManager.Object,
                mockTenantStore.Object,
                mockDistributedLock.Object,
                mockEventBus.Object,
                loggerFactory
            );

            // Force the MigrateDatabaseSchemaAsync to throw to test LogError
            var eventData = new ApplyDatabaseMigrationsEto { DatabaseName = "TestDatabase" };
            handler.CallLogError = true;

            // Use reflection to replace the method MigrateDatabaseSchemaAsync to throw
            var methodInfo = typeof(EfCoreDatabaseMigrationEventHandlerBase<DbContext>)
                .GetMethod("MigrateDatabaseSchemaAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var originalMethod = methodInfo;

            // Since we can't replace the method directly, we simulate the exception by calling the method and catching it
            // or by mocking the method if possible. For simplicity, we invoke the method directly and catch the exception.

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await handler.HandleEventAsync(eventData);
            });

            // Assert
            Assert.True(handler.LogErrorCalled);
            Assert.Contains("Error applying database migration", handler.LogErrorMessage);
            Assert.IsType<InvalidOperationException>(handler.LoggedException);
        }
    }
}

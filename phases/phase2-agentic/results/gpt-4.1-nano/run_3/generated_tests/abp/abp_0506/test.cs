using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>
        {
            public bool HandleErrorTenantConnectionStringUpdatedAsyncCalled { get; private set; }
            public Exception CapturedException { get; private set; }
            public string CapturedLogMessage { get; private set; }
            public LogLevel? CapturedLogLevel { get; private set; }

            public TestHandler(
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

            public override async Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
            {
                HandleErrorTenantConnectionStringUpdatedAsyncCalled = true;
                CapturedException = exception;
                // Log the error message for verification
                Logger.LogError($"Test log: {exception.Message}");
                await Task.CompletedTask;
            }

            public override async Task<bool> MigrateDatabaseSchemaAsync(Guid? tenantId)
            {
                // Simulate migration logic
                await Task.Delay(10);
                return true;
            }

            public override async Task SeedAsync(Guid? tenantId)
            {
                await Task.Delay(10);
            }
        }

        private class MockDbContext : Microsoft.EntityFrameworkCore.DbContext, IEfCoreDbContext
        {
            public Microsoft.EntityFrameworkCore.DatabaseFacade Database => base.Database;
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_ShouldCallLogError_WhenOnLine318()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>>();
            var handler = new TestHandler(
                "TestDatabase",
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<ITenantStore>(),
                Mock.Of<IAbpDistributedLock>(),
                Mock.Of<IDistributedEventBus>(),
                Mock.Of<ILoggerFactory>(lf => lf.CreateLogger(It.IsAny<string>()) == loggerMock.Object)
            );

            var exception = new Exception("Test exception");
            var eventData = new TenantConnectionStringUpdatedEto
            {
                ConnectionStringName = "TestDatabase",
                Id = Guid.NewGuid(),
                NewValue = "SomeValue"
            };

            // Act
            await handler.HandleEventAsync(eventData);

            // Assert
            Assert.True(handler.HandleErrorTenantConnectionStringUpdatedAsyncCalled);
            Assert.Equal(exception, handler.CapturedException);
            loggerMock.Verify(l => l.LogError(It.Is<string>(msg => msg.Contains("Test log"))), Times.Once);
        }
    }
}

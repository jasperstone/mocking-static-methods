using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Volo.Abp.Domain.Entities.Events.Distributed;

namespace Volo.Abp.EntityFrameworkCore.Tests
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

            public override Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
            {
                // Call base to test LogError
                return base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
            }
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_LogError_When_TryCount_Exceeds_Max()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<DbContext>>>();
            var handler = new DummyHandler(
                "TestDb",
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<ITenantStore>(),
                Mock.Of<IAbpDistributedLock>(),
                Mock.Of<IDistributedEventBus>(),
                new LoggerFactory()
            );
            // Inject mock logger
            typeof(EfCoreDatabaseMigrationEventHandlerBase<DbContext>)
                .GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(handler, loggerMock.Object);

            var eventData = new TenantConnectionStringUpdatedEto
            {
                ConnectionStringName = "TestDb",
                Id = Guid.NewGuid(),
                NewValue = "SomeValue",
                Properties = new System.Collections.Generic.Dictionary<string, string> { { "__TryCount", "4" } }
            };
            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
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

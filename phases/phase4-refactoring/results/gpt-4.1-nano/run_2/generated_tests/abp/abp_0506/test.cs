using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Uow;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Microsoft.EntityFrameworkCore;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class SampleDbContext : DbContext
        {
            public SampleDbContext(DbContextOptions options) : base(options) { }
        }

        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<SampleDbContext>
        {
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

            public override Task<bool> MigrateDatabaseSchemaAsync(Guid? tenantId)
            {
                return Task.FromResult(true);
            }

            public override Task SeedAsync(Guid? tenantId)
            {
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_LogError_When_TryCount_Exceeds_Max()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<SampleDbContext>>>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var tenantStoreMock = new Mock<ITenantStore>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();
            var eventBusMock = new Mock<IDistributedEventBus>();
            var loggerFactory = new LoggerFactory();

            var handler = new TestHandler(
                "TestDatabase",
                currentTenantMock.Object,
                unitOfWorkManagerMock.Object,
                tenantStoreMock.Object,
                distributedLockMock.Object,
                eventBusMock.Object,
                loggerFactory
            );

            // Inject the mock logger into the handler via reflection
            var loggerField = typeof(EfCoreDatabaseMigrationEventHandlerBase<SampleDbContext>)
                .GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(handler, loggerMock.Object);

            var eventData = new TenantConnectionStringUpdatedEto
            {
                ConnectionStringName = "TestDatabase",
                Id = Guid.NewGuid(),
                NewValue = "SomeValue",
                Properties = new Dictionary<string, string>()
            };

            // Set the try count to exceed MaxEventTryCount
            eventData.Properties["__TryCount"] = (handler.MaxEventTryCount + 1).ToString();

            // Act
            await handler.HandleEventAsync(eventData);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant connection string updated event. Canceling the operation.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}

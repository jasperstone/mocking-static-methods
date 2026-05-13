using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<DbContextMock>
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

            protected override Task<bool> MigrateDatabaseSchemaAsync(Guid? tenantId)
            {
                return Task.FromResult(true);
            }
        }

        private class DbContextMock { }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsErrorOnMaxTryCountExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<DbContextMock>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var tenantStoreMock = new Mock<ITenantStore>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();

            var handler = new TestHandler(
                "TestDb",
                currentTenantMock.Object,
                unitOfWorkManagerMock.Object,
                tenantStoreMock.Object,
                distributedLockMock.Object,
                distributedEventBusMock.Object,
                loggerFactoryMock.Object);

            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TenantName",
                ConnectionStringName = "TestDb",
                NewValue = "NewConnectionString",
                Properties = { [EfCoreDatabaseMigrationEventHandlerBase<DbContextMock>.TryCountPropertyName] = "3" }
            };

            var exception = new Exception("Test exception");

            // Act
            // Use reflection to call the protected method HandleErrorTenantConnectionStringUpdatedAsync
            var method = typeof(EfCoreDatabaseMigrationEventHandlerBase<DbContextMock>)
                .GetMethod("HandleErrorTenantConnectionStringUpdatedAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.NotNull(method);

            var task = (Task)method.Invoke(handler, new object[] { eventData, exception })!;
            await task;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Canceling the operation")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal Eto classes for testing
    public class TenantConnectionStringUpdatedEto : EtoBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string ConnectionStringName { get; set; } = "";
        public string NewValue { get; set; } = "";
    }

    public class EtoBase
    {
        public System.Collections.Generic.Dictionary<string, string> Properties { get; } = new();
    }
}

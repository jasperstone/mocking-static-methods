using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<DbContextStub>
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

            protected override Task SeedAsync(Guid? tenantId)
            {
                return Task.CompletedTask;
            }
        }

        private class DbContextStub : Microsoft.EntityFrameworkCore.DbContext, IEfCoreDbContext
        {
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_LogError_When_TryCount_Exceeds_Max()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<DbContextStub>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var tenantStoreMock = new Mock<ITenantStore>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();

            var handler = new TestHandler(
                "TestDatabase",
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
                ConnectionStringName = "TestDatabase",
                NewValue = "SomeConnectionString",
                Properties = { ["__TryCount"] = "3" } // MaxEventTryCount is 3, so increment will be 4 > 3
            };

            var exception = new Exception("Test exception");

            // Act
            // We call the protected method via reflection because it's protected
            var method = typeof(EfCoreDatabaseMigrationEventHandlerBase<DbContextStub>)
                .GetMethod("HandleErrorTenantConnectionStringUpdatedAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task)method.Invoke(handler, new object[] { eventData, exception })!;
            await task;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Could not perform tenant connection string updated event. Canceling the operation")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

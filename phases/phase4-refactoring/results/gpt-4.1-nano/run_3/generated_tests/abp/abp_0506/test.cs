using System;
using System.Collections.Generic;
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
        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<SampleDbContext>
        {
            public bool HandleErrorCalled { get; private set; }
            public string? LoggedErrorMessage { get; private set; }

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

            public override Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
            {
                HandleErrorCalled = true;
                LoggedErrorMessage = $"Error: {exception.Message}";
                // Call base to perform logging
                return base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
            }
        }

        private class SampleDbContext : Microsoft.EntityFrameworkCore.DbContext, IEfCoreDbContext
        {
            public Microsoft.EntityFrameworkCore.DatabaseFacade Database => base.Database;
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_LogError_When_MaxTryCountExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<SampleDbContext>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<SampleDbContext>>())
                .Returns(loggerMock.Object);

            var currentTenantMock = Mock.Of<ICurrentTenant>();
            var unitOfWorkManagerMock = Mock.Of<IUnitOfWorkManager>();
            var tenantStoreMock = Mock.Of<ITenantStore>();
            var distributedLockMock = Mock.Of<IAbpDistributedLock>();
            var eventBusMock = Mock.Of<IDistributedEventBus>();

            var handler = new TestHandler(
                "TestDatabase",
                currentTenantMock,
                unitOfWorkManagerMock,
                tenantStoreMock,
                distributedLockMock,
                eventBusMock,
                loggerFactoryMock.Object);

            // Create event data with try count exceeding max
            var eventData = new TenantConnectionStringUpdatedEto
            {
                ConnectionStringName = "TestDatabase",
                Id = Guid.NewGuid(),
                NewValue = "SomeValue",
                Properties = new Dictionary<string, string>
                {
                    { "__TryCount", "4" }
                }
            };

            // Simulate incrementing try count to exceed max
            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Could not perform tenant connection string updated event. Canceling the operation.")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(handler.HandleErrorCalled);
            Assert.Contains("Could not perform tenant connection string updated event. Canceling the operation.", handler.LoggedErrorMessage);
        }
    }
}

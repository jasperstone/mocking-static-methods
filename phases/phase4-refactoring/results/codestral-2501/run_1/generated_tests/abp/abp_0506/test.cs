using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_ShouldLogError_WhenMaxTryCountExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();
            var eventData = new TenantConnectionStringUpdatedEto { Id = Guid.NewGuid(), Name = "TestTenant" };
            var exception = new Exception("Test exception");

            var handler = new TestEfCoreDatabaseMigrationEventHandler(
                "TestDatabase",
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<ITenantStore>(),
                Mock.Of<IAbpDistributedLock>(),
                distributedEventBusMock.Object,
                Mock.Of<ILoggerFactory>());

            handler.Logger = loggerMock.Object;
            handler.MaxEventTryCount = 1; // Set max try count to 1 for testing

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant connection string updated event. Canceling the operation.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }

    public class TestDbContext : DbContext, IEfCoreDbContext
    {
        public void Attach<TEntity>(TEntity entity) where TEntity : class
        {
            // Implementation not needed for the test
        }

        public void Attach(object entity)
        {
            // Implementation not needed for the test
        }

        public Task SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            // Implementation not needed for the test
            return Task.CompletedTask;
        }
    }

    public class TestEfCoreDatabaseMigrationEventHandler : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
    {
        public TestEfCoreDatabaseMigrationEventHandler(
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
}

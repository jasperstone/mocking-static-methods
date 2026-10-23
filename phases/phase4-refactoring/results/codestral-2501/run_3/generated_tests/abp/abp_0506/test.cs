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
        private readonly Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>> _loggerMock;
        private readonly Mock<IDistributedEventBus> _distributedEventBusMock;
        private readonly TestEfCoreDatabaseMigrationEventHandler _handler;

        public EfCoreDatabaseMigrationEventHandlerBaseTests()
        {
            _loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            _distributedEventBusMock = new Mock<IDistributedEventBus>();
            _handler = new TestEfCoreDatabaseMigrationEventHandler(
                "TestDatabase",
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<ITenantStore>(),
                Mock.Of<IAbpDistributedLock>(),
                _distributedEventBusMock.Object,
                Mock.Of<ILoggerFactory>(),
                _loggerMock.Object);
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_ShouldLogError_WhenMaxTryCountExceeded()
        {
            // Arrange
            var eventData = new TenantConnectionStringUpdatedEto { Id = Guid.NewGuid(), Name = "TestTenant" };
            var exception = new Exception("Test exception");

            // Act
            await _handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant connection string updated event. Canceling the operation.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }

    public class TestDbContext : DbContext, IEfCoreDbContext
    {
        public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
            ILoggerFactory loggerFactory,
            ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>> logger)
            : base(databaseName, currentTenant, unitOfWorkManager, tenantStore, abpDistributedLock, distributedEventBus, loggerFactory)
        {
            Logger = logger;
        }

        public override async Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
        {
            await base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
        }
    }
}

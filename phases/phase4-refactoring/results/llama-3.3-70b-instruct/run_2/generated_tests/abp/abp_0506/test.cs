using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var tenantStoreMock = new Mock<ITenantStore>();
            var distributedLockMock = new Mock<IDistributedLock>();

            var handler = new TestEfCoreDatabaseMigrationEventHandlerBase(
                "DatabaseName",
                currentTenantMock.Object,
                unitOfWorkManagerMock.Object,
                tenantStoreMock.Object,
                distributedLockMock.Object,
                distributedEventBusMock.Object,
                new LoggerFactory().CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>()
            );

            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                ConnectionStringName = "ConnectionStringName",
                NewValue = "NewValue"
            };

            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }

    public class TestEfCoreDatabaseMigrationEventHandlerBase : EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>
    {
        public TestEfCoreDatabaseMigrationEventHandlerBase(
            string databaseName,
            ICurrentTenant currentTenant,
            IUnitOfWorkManager unitOfWorkManager,
            ITenantStore tenantStore,
            IDistributedLock distributedLock,
            IDistributedEventBus distributedEventBus,
            ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>> logger)
            : base(
                databaseName,
                currentTenant,
                unitOfWorkManager,
                tenantStore,
                distributedLock,
                distributedEventBus,
                logger)
        {
        }

        public override async Task HandleErrorTenantConnectionStringUpdatedAsync(
            TenantConnectionStringUpdatedEto eventData,
            Exception exception)
        {
            await base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
        }
    }

    public class MockDbContext : DbContext, IEfCoreDbContext
    {
        public MockDbContext(DbContextOptions<MockDbContext> options) : base(options)
        {
        }

        public override int SaveChanges()
        {
            return 0;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return 0;
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public void Attach<TEntity>(TEntity entity) where TEntity : class
        {
        }

        public void Attach(object entity)
        {
        }

        public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}

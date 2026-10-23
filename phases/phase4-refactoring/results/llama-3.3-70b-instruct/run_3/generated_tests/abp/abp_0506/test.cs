using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.EntityFrameworkCore.Migrations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using System.Threading;
using Volo.Abp.Domain.Entities.Events.Distributed;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsError()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var handler = new TestEfCoreDatabaseMigrationEventHandlerBase(
                "TestDatabase",
                null,
                null,
                null,
                null,
                null,
                loggerFactoryMock.Object);

            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "Test Tenant"
            };

            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }
    }

    public class TestEfCoreDatabaseMigrationEventHandlerBase : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
    {
        public TestEfCoreDatabaseMigrationEventHandlerBase(
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

        public async Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
        {
            await base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
        }
    }

    public class TestDbContext : DbContext, IEfCoreDbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        public void Attach<TEntity>(TEntity entity) where TEntity : class
        {
            base.Attach(entity);
        }

        public void Attach(object entity)
        {
            base.Attach(entity);
        }

        public int SaveChanges()
        {
            return base.SaveChanges();
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public async Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}

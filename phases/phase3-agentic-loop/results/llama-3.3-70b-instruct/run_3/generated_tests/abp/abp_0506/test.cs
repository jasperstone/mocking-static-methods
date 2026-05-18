using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsErrorAndException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();
            var handler = new TestEfCoreDatabaseMigrationEventHandlerBase(loggerMock.Object, distributedEventBusMock.Object);

            var eventData = new TenantConnectionStringUpdatedEto { Id = Guid.NewGuid(), Name = "Test Tenant" };
            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
            loggerMock.Verify(l => l.LogException(exception), Times.Once);
        }

        private class TestEfCoreDatabaseMigrationEventHandlerBase : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
        {
            public TestEfCoreDatabaseMigrationEventHandlerBase(ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>> logger, IDistributedEventBus distributedEventBus)
                : base("TestDatabase", null, null, null, null, distributedEventBus, null)
            {
                Logger = logger;
            }
        }

        private class TestDbContext : DbContext, IEfCoreDbContext
        {
            public TestDbContext(DbContextOptions options) : base(options)
            {
            }

            public void Attach(object entity)
            {
                throw new NotImplementedException();
            }

            public void Attach<TEntity>(TEntity entity)
            {
                throw new NotImplementedException();
            }

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
    }
}

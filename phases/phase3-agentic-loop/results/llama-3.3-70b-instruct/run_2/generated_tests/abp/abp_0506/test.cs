using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        [Fact]
        public async Task HandleErrorTenantCreatedAsync_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var handler = new TestEfCoreDatabaseMigrationEventHandlerBase(loggerFactoryMock.Object);

            var eventData = new TenantCreatedEto { Id = Guid.NewGuid(), Name = "Test Tenant" };
            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantCreatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var handler = new TestEfCoreDatabaseMigrationEventHandlerBase(loggerFactoryMock.Object);

            var eventData = new TenantConnectionStringUpdatedEto { Id = Guid.NewGuid(), Name = "Test Tenant" };
            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        private class TestEfCoreDatabaseMigrationEventHandlerBase : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
        {
            public TestEfCoreDatabaseMigrationEventHandlerBase(ILoggerFactory loggerFactory)
                : base("Test Database", null, null, null, null, null, loggerFactory)
            {
            }

            public async Task HandleErrorTenantCreatedAsync(TenantCreatedEto eventData, Exception exception)
            {
                await base.HandleErrorTenantCreatedAsync(eventData, exception);
            }

            public async Task HandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
            {
                await base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
            }
        }

        private class TestDbContext : DbContext, IEfCoreDbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
            {
            }

            public override int SaveChanges()
            {
                return base.SaveChanges();
            }

            public new DbSet<TEntity> Set<TEntity>() where TEntity : class
            {
                return base.Set<TEntity>();
            }

            public void Attach<TEntity>(TEntity entity) where TEntity : class
            {
                base.Attach(entity);
            }

            public async Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
        }
    }
}

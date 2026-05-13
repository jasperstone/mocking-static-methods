using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            var handler = new TestEfCoreDatabaseMigrationEventHandlerBase(loggerMock.Object);

            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "Test Tenant"
            };

            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<Exception, string>>(),
                    exception),
                Times.Once);
        }

        private class TestEfCoreDatabaseMigrationEventHandlerBase : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
        {
            public TestEfCoreDatabaseMigrationEventHandlerBase(ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>> logger)
                : base("TestDatabase", null, null, null, null, null, logger)
            {
            }
        }

        private class TestDbContext : DbContext, IEfCoreDbContext
        {
            public TestDbContext(DbContextOptions options) : base(options)
            {
            }
        }
    }
}

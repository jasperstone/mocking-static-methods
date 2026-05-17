using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.DistributedEventBus;
using Volo.Abp.MultiTenancy;
using Volo.Abp.EntityFrameworkCore;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private readonly Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>> _loggerMock;
        private readonly Mock<IDistributedEventBus> _distributedEventBusMock;
        private readonly Mock<IRandomHelper> _randomHelperMock;
        private readonly EfCoreDatabaseMigrationEventHandlerBase<MockDbContext> _handler;

        public EfCoreDatabaseMigrationEventHandlerBaseTests()
        {
            _loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>>();
            _distributedEventBusMock = new Mock<IDistributedEventBus>();
            _randomHelperMock = new Mock<IRandomHelper>();

            _handler = new EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>(
                "TestDatabase",
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<ITenantStore>(),
                Mock.Of<IAbpDistributedLock>(),
                _distributedEventBusMock.Object,
                Mock.Of<ILoggerFactory>())
            {
                Logger = _loggerMock.Object,
                RandomHelper = _randomHelperMock.Object
            };
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsError_WhenTryCountExceedsMax()
        {
            // Arrange
            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TestTenant",
                Properties = { { "__TryCount", (MaxEventTryCount + 1).ToString() } }
            };
            var exception = new Exception("Test exception");

            // Act
            await _handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Could not perform tenant connection string updated event. Canceling the operation.")),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );

            _distributedEventBusMock.Verify(
                bus => bus.PublishAsync(It.IsAny<TenantConnectionStringUpdatedEto>()),
                Times.Never
            );
        }
    }

    // Mock DbContext for testing purposes
    public class MockDbContext : DbContext, IEfCoreDbContext
    {
        public MockDbContext(DbContextOptions<MockDbContext> options) : base(options) { }

        public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}

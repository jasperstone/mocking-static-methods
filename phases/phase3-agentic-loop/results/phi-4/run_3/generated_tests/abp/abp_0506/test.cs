using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Data;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private readonly Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>> _loggerMock;
        private readonly Mock<IDistributedEventBus> _distributedEventBusMock;
        private readonly Mock<IAbpDistributedLock> _distributedLockMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<ITenantStore> _tenantStoreMock;
        private readonly EfCoreDatabaseMigrationEventHandlerBase<MockDbContext> _handler;

        public EfCoreDatabaseMigrationEventHandlerBaseTests()
        {
            _loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>>();
            _distributedEventBusMock = new Mock<IDistributedEventBus>();
            _distributedLockMock = new Mock<IAbpDistributedLock>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _tenantStoreMock = new Mock<ITenantStore>();

            _handler = new EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>(
                "TestDatabase",
                _currentTenantMock.Object,
                _unitOfWorkManagerMock.Object,
                _tenantStoreMock.Object,
                _distributedLockMock.Object,
                _distributedEventBusMock.Object,
                Mock.Of<ILoggerFactory>(f => f.CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>() == _loggerMock.Object));
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsError_WhenTryCountExceedsMax()
        {
            // Arrange
            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TestTenant",
                Properties = { { "__TryCount", "4" } } // Exceeds MaxEventTryCount
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
                Times.Once);
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

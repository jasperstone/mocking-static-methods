using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.DistributedEventBus;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private readonly Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<object>>> _loggerMock;
        private readonly Mock<IDistributedEventBus> _distributedEventBusMock;
        private readonly Mock<IRandomHelper> _randomHelperMock;
        private readonly EfCoreDatabaseMigrationEventHandlerBase<object> _handler;

        public EfCoreDatabaseMigrationEventHandlerBaseTests()
        {
            _loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<object>>>();
            _distributedEventBusMock = new Mock<IDistributedEventBus>();
            _randomHelperMock = new Mock<IRandomHelper>();

            _handler = new EfCoreDatabaseMigrationEventHandlerBase<object>(
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
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsErrorOnMaxTryCount()
        {
            // Arrange
            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TestTenant",
                Properties = new Dictionary<string, string> { { "__TryCount", "3" } }
            };
            var exception = new Exception("Test exception");

            // Act
            await _handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains($"Could not perform tenant connection string updated event. Canceling the operation. TenantId = {eventData.Id}, TenantName = {eventData.Name}.")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }
}

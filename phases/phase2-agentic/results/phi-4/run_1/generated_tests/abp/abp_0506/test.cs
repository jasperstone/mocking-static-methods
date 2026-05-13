using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DistributedEventBus;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsError_WhenMaxTryCountExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<DbContext>>>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();
            var randomHelperMock = new Mock<IRandomHelper>();

            var handler = new EfCoreDatabaseMigrationEventHandlerBase<DbContext>(
                "TestDatabase",
                null, // Mock or provide necessary dependencies
                null,
                null,
                null,
                distributedEventBusMock.Object,
                new LoggerFactory().AddProvider(new TestLoggerProvider(loggerMock.Object))
            )
            {
                MaxEventTryCount = 1 // Set to 1 to trigger the error logging
            };

            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TestTenant",
                Properties = new Dictionary<string, string>
                {
                    { "__TryCount", "1" } // Set try count to exceed MaxEventTryCount
                }
            };

            var exception = new Exception("Test exception");

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Could not perform tenant connection string updated event. Canceling the operation.")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }

    // Mock interface for RandomHelper
    public interface IRandomHelper
    {
        int GetRandom(int minValue, int maxValue);
    }

    // Mock implementation for RandomHelper
    public class RandomHelperMock : IRandomHelper
    {
        public int GetRandom(int minValue, int maxValue) => minValue;
    }
}

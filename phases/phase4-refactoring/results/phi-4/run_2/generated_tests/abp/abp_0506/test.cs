using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DistributedEventBus;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

public class EfCoreDatabaseMigrationEventHandlerBaseTests
{
    [Fact]
    public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsErrorWhenOperationCanceled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>>();
        var distributedEventBusMock = new Mock<IDistributedEventBus>();
        var randomHelperMock = new Mock<IRandomHelper>();
        var tenantStoreMock = new Mock<ITenantStore>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var currentTenantMock = new Mock<ICurrentTenant>();
        var distributedLockMock = new Mock<IAbpDistributedLock>();

        var eventData = new TenantConnectionStringUpdatedEto
        {
            Id = Guid.NewGuid(),
            Name = "TestTenant",
            Properties = new Dictionary<string, string>()
        };

        var exception = new Exception("Test exception");

        var handler = new EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>(
            "TestDatabase",
            currentTenantMock.Object,
            unitOfWorkManagerMock.Object,
            tenantStoreMock.Object,
            distributedLockMock.Object,
            distributedEventBusMock.Object,
            new LoggerFactory().AddProvider(new TestLoggerProvider())
        )
        {
            Logger = loggerMock.Object,
            RandomHelper = randomHelperMock.Object
        };

        // Act
        await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains($"Could not perform tenant connection string updated event. Canceling the operation. TenantId = {eventData.Id}, TenantName = {eventData.Name}.")),
                It.IsAny<Exception>()
            ),
            Times.Once
        );
    }
}

// Mock classes for testing
public class MockDbContext : DbContext, IEfCoreDbContext { }
public interface IRandomHelper
{
    int GetRandom(int min, int max);
}
public class TestLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new TestLogger();
    public void Dispose() { }
}
public class TestLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        // No-op
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private readonly Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>> _loggerMock;
        private readonly Mock<IDistributedEventBus> _distributedEventBusMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<ITenantStore> _tenantStoreMock;
        private readonly Mock<IAbpDistributedLock> _distributedLockMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;

        private readonly EfCoreDatabaseMigrationEventHandlerBase<TestDbContext> _handler;

        public EfCoreDatabaseMigrationEventHandlerBaseTests()
        {
            _loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            _distributedEventBusMock = new Mock<IDistributedEventBus>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _tenantStoreMock = new Mock<ITenantStore>();
            _distributedLockMock = new Mock<IAbpDistributedLock>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();

            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

            _handler = new TestEfCoreDatabaseMigrationEventHandler(
                "TestDatabase",
                _currentTenantMock.Object,
                _unitOfWorkManagerMock.Object,
                _tenantStoreMock.Object,
                _distributedLockMock.Object,
                _distributedEventBusMock.Object,
                _loggerFactoryMock.Object
            );
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_ShouldLogErrorAndException_WhenMaxTryCountExceeded()
        {
            // Arrange
            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TestTenant",
                ConnectionStringName = "TestDatabase",
                NewValue = "TestConnectionString"
            };
            var exception = new Exception("Test exception");

            // Act
            await _handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant connection string updated event. Canceling the operation.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test exception")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }

    public class TestEfCoreDatabaseMigrationEventHandler : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
    {
        public TestEfCoreDatabaseMigrationEventHandler(
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
    }

    public class TestDbContext : DbContext, IEfCoreDbContext
    {
    }
}

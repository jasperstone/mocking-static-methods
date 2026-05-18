using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;
using Volo.Abp.DistributedLocking;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Volo.Abp.Domain.Entities;

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
        private readonly EfCoreDatabaseMigrationEventHandlerBase<TestDbContext> _handler;

        public EfCoreDatabaseMigrationEventHandlerBaseTests()
        {
            _loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            _distributedEventBusMock = new Mock<IDistributedEventBus>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _tenantStoreMock = new Mock<ITenantStore>();
            _distributedLockMock = new Mock<IAbpDistributedLock>();

            _handler = new TestEfCoreDatabaseMigrationEventHandler(
                "TestDatabase",
                _currentTenantMock.Object,
                _unitOfWorkManagerMock.Object,
                _tenantStoreMock.Object,
                _distributedLockMock.Object,
                _distributedEventBusMock.Object,
                Mock.Of<ILoggerFactory>()
            );
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_ShouldLogError_WhenMaxTryCountExceeded()
        {
            // Arrange
            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TestTenant"
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
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
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
        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Entry(object entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public int SaveChanges()
        {
            throw new NotImplementedException();
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            throw new NotImplementedException();
        }

        public DatabaseFacade Database => throw new NotImplementedException();

        public DbContext DbContext => throw new NotImplementedException();

        public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Attach(object entity)
        {
            throw new NotImplementedException();
        }

        public ChangeTracker ChangeTracker => throw new NotImplementedException();
    }
}

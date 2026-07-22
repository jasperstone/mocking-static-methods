using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class DbContextFake : DbContext, IEfCoreDbContext
        {
            public new void Dispose() { }

            IServiceProvider IInfrastructure<IServiceProvider>.Instance => throw new NotImplementedException();

            public int SaveChanges() => throw new NotImplementedException();

            public int SaveChanges(bool acceptAllChangesOnSuccess) => throw new NotImplementedException();

            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => Task.FromResult(0);

            public DbSet<T> Set<T>() where T : class => throw new NotImplementedException();

            public DbSet<T> Set<T>(string name) where T : class => throw new NotImplementedException();

            public DatabaseFacade Database => throw new NotImplementedException();

            public ChangeTracker ChangeTracker => throw new NotImplementedException();

            public EntityEntry Add(object entity) => throw new NotImplementedException();

            public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();

            public ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class => throw new NotImplementedException();

            public void AddRange(params object[] entities) => throw new NotImplementedException();

            public void AddRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();

            public Task AddRangeAsync(params object[] entities) => throw new NotImplementedException();

            public Task AddRangeAsync(System.Collections.Generic.IEnumerable<object> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public EntityEntry Attach(object entity) => throw new NotImplementedException();

            public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();

            public void AttachRange(params object[] entities) => throw new NotImplementedException();

            public void AttachRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();

            public EntityEntry Entry(object entity) => throw new NotImplementedException();

            public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();

            public object Find(Type entityType, params object[] keyValues) => throw new NotImplementedException();

            public TEntity Find<TEntity>(params object[] keyValues) where TEntity : class => throw new NotImplementedException();

            public ValueTask<object> FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken) => throw new NotImplementedException();

            public ValueTask<TEntity> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class => throw new NotImplementedException();

            public ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class => throw new NotImplementedException();

            public ValueTask<object> FindAsync(Type entityType, params object[] keyValues) => throw new NotImplementedException();

            public EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();

            public EntityEntry Remove(object entity) => throw new NotImplementedException();

            public void RemoveRange(params object[] entities) => throw new NotImplementedException();

            public void RemoveRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();

            public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();

            public EntityEntry Update(object entity) => throw new NotImplementedException();

            public void UpdateRange(params object[] entities) => throw new NotImplementedException();

            public void UpdateRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
        }

        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<DbContextFake>
        {
            public TestHandler(
                ICurrentTenant currentTenant,
                IUnitOfWorkManager unitOfWorkManager,
                ITenantStore tenantStore,
                IAbpDistributedLock abpDistributedLock,
                IDistributedEventBus distributedEventBus,
                ILoggerFactory loggerFactory)
                : base("TestDatabase", currentTenant, unitOfWorkManager, tenantStore, abpDistributedLock, distributedEventBus, loggerFactory)
            {
            }

            protected override Task<bool> MigrateDatabaseSchemaAsync(Guid? tenantId)
            {
                return Task.FromResult(true);
            }

            protected override Task SeedAsync(Guid? tenantId)
            {
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsErrorAndException_WhenTryCountExceedsMax()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<DbContextFake>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);

            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var tenantStoreMock = new Mock<ITenantStore>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();

            var handler = new TestHandler(
                currentTenantMock.Object,
                unitOfWorkManagerMock.Object,
                tenantStoreMock.Object,
                distributedLockMock.Object,
                distributedEventBusMock.Object,
                loggerFactoryMock.Object);

            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TenantName",
                ConnectionStringName = "TestDatabase",
                NewValue = "NewConnectionString"
            };

            // Set try count to MaxEventTryCount + 1 to trigger error logging branch
            for (int i = 0; i <= handler.MaxEventTryCount; i++)
            {
                var incrementMethod = typeof(EfCoreDatabaseMigrationEventHandlerBase<DbContextFake>)
                    .GetMethod("IncrementEventTryCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                incrementMethod.Invoke(null, new object[] { eventData });
            }

            var exception = new Exception("Test exception");

            // Act
            var method = typeof(EfCoreDatabaseMigrationEventHandlerBase<DbContextFake>)
                .GetMethod("HandleErrorTenantConnectionStringUpdatedAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            await (Task)method.Invoke(handler, new object[] { eventData, exception });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Canceling the operation")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    exception,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }

    public class TenantConnectionStringUpdatedEto : EtoBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ConnectionStringName { get; set; }
        public string NewValue { get; set; }
    }

    public class EtoBase
    {
        public System.Collections.Generic.Dictionary<string, string> Properties { get; } = new System.Collections.Generic.Dictionary<string, string>();
    }
}

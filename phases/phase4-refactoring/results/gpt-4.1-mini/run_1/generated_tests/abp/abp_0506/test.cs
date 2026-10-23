using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class TestDbContext : DbContext, IEfCoreDbContext
        {
            public void Dispose() { }
            public IServiceProvider Instance => null!;
            public IDbContextDependencies Dependencies => null!;
            public IDbSetCache SetCache => null!;
            public bool IsInMemory => false;

            public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class => null!;
            public EntityEntry Attach(object entity) => null!;
            public int SaveChanges() => 0;
            public int SaveChanges(bool acceptAllChangesOnSuccess) => 0;
            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(0);
            public DbSet<T> Set<T>() where T : class => null!;
            public DbSet<T> Set<T>(string name) where T : class => null!;
            public DatabaseFacade Database => null!;
            public ChangeTracker ChangeTracker => null!;
            public EntityEntry Add(object entity) => null!;
            public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class => null!;
            public ValueTask<EntityEntry> AddAsync(object entity, System.Threading.CancellationToken cancellationToken = default) => new ValueTask<EntityEntry>(null!);
            public ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, System.Threading.CancellationToken cancellationToken = default) where TEntity : class => new ValueTask<EntityEntry<TEntity>>(null!);
            public void AddRange(params object[] entities) { }
            public void AddRange(System.Collections.Generic.IEnumerable<object> entities) { }
            public Task AddRangeAsync(params object[] entities) => Task.CompletedTask;
            public Task AddRangeAsync(System.Collections.Generic.IEnumerable<object> entities, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void AttachRange(params object[] entities) { }
            public void AttachRange(System.Collections.Generic.IEnumerable<object> entities) { }
            public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => null!;
            public EntityEntry Entry(object entity) => null!;
            public object? Find(Type entityType, params object[] keyValues) => null;
            public TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class => null;
            public ValueTask<object?> FindAsync(Type entityType, object[] keyValues, System.Threading.CancellationToken cancellationToken) => new ValueTask<object?>(null);
            public ValueTask<TEntity?> FindAsync<TEntity>(object[] keyValues, System.Threading.CancellationToken cancellationToken) where TEntity : class => new ValueTask<TEntity?>(null);
            public ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class => new ValueTask<TEntity?>(null);
            public ValueTask<object?> FindAsync(Type entityType, params object[] keyValues) => new ValueTask<object?>(null);
            public EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class => null!;
            public EntityEntry Remove(object entity) => null!;
            public void RemoveRange(System.Collections.Generic.IEnumerable<object> entities) { }
            public void RemoveRange(params object[] entities) { }
            public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class => null!;
            public EntityEntry Update(object entity) => null!;
            public void UpdateRange(params object[] entities) { }
            public void UpdateRange(System.Collections.Generic.IEnumerable<object> entities) { }
        }

        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
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
                return Task.FromResult(false);
            }
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsErrorAndException_WhenTryCountExceedsMax()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
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
                NewValue = "SomeValue"
            };
            // Set try count to MaxEventTryCount to trigger the else branch
            eventData.Properties["__TryCount"] = "3";

            var exception = new Exception("Test exception");

            // Act
            var method = typeof(EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>)
                .GetMethod("HandleErrorTenantConnectionStringUpdatedAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)method.Invoke(handler, new object[] { eventData, exception });
            await task;

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant connection string updated event. Canceling the operation")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    public class TenantConnectionStringUpdatedEto : EtoBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string ConnectionStringName { get; set; } = "";
        public string NewValue { get; set; } = "";
    }

    public class EtoBase
    {
        public EtoBase()
        {
            Properties = new System.Collections.Generic.Dictionary<string, string>();
        }
        public System.Collections.Generic.IDictionary<string, string> Properties { get; }
    }
}

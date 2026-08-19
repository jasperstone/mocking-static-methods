using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<DbContextStub>
        {
            public TestHandler(
                ICurrentTenant currentTenant,
                IUnitOfWorkManager unitOfWorkManager,
                ITenantStore tenantStore,
                object abpDistributedLock,
                IDistributedEventBus distributedEventBus,
                ILoggerFactory loggerFactory)
                : base("TestDatabase", currentTenant, unitOfWorkManager, tenantStore, null!, distributedEventBus, loggerFactory)
            {
            }

            protected override Task<bool> MigrateDatabaseSchemaAsync(Guid? tenantId)
            {
                // Simulate migration always false to trigger error handling
                return Task.FromResult(false);
            }

            protected override Task SeedAsync(Guid? tenantId)
            {
                return Task.CompletedTask;
            }

            public async Task CallHandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception ex)
            {
                await HandleErrorTenantConnectionStringUpdatedAsync(eventData, ex);
            }
        }

        private class DbContextStub : IDisposable, IEfCoreDbContext
        {
            public void Dispose() { }
            public IServiceProvider Instance => null!;
            public object Database => null!;
            public object ChangeTracker => null!;
            public object Attach<TEntity>(TEntity entity) where TEntity : class => null!;
            public object Attach(object entity) => null!;
            public int SaveChanges() => 0;
            public int SaveChanges(bool acceptAllChangesOnSuccess) => 0;
            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(0);
            public object Set<T>() where T : class => null!;
            public object Set<T>(string name) where T : class => null!;
            public object Add(object entity) => null!;
            public object Add<TEntity>(TEntity entity) where TEntity : class => null!;
            public ValueTask<object> AddAsync(object entity, System.Threading.CancellationToken cancellationToken = default) => new ValueTask<object>((object?)null!);
            public ValueTask<object> AddAsync<TEntity>(TEntity entity, System.Threading.CancellationToken cancellationToken = default) where TEntity : class => new ValueTask<object>((object?)null!);
            public void AddRange(params object[] entities) { }
            public void AddRange(System.Collections.Generic.IEnumerable<object> entities) { }
            public Task AddRangeAsync(params object[] entities) => Task.CompletedTask;
            public Task AddRangeAsync(System.Collections.Generic.IEnumerable<object> entities, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void AttachRange(params object[] entities) { }
            public void AttachRange(System.Collections.Generic.IEnumerable<object> entities) { }
            public object Entry<TEntity>(TEntity entity) where TEntity : class => null!;
            public object Entry(object entity) => null!;
            public object? Find(Type entityType, params object[] keyValues) => null;
            public TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class => null;
            public ValueTask<object?> FindAsync(Type entityType, object[] keyValues, System.Threading.CancellationToken cancellationToken) => new ValueTask<object?>((object?)null);
            public ValueTask<TEntity?> FindAsync<TEntity>(object[] keyValues, System.Threading.CancellationToken cancellationToken) where TEntity : class => new ValueTask<TEntity?>((TEntity?)null);
            public ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class => new ValueTask<TEntity?>((TEntity?)null);
            public ValueTask<object?> FindAsync(Type entityType, params object[] keyValues) => new ValueTask<object?>((object?)null);
            public object Remove<TEntity>(TEntity entity) where TEntity : class => null!;
            public object Remove(object entity) => null!;
            public void RemoveRange(params object[] entities) { }
            public void RemoveRange(System.Collections.Generic.IEnumerable<object> entities) { }
            public object Update<TEntity>(TEntity entity) where TEntity : class => null!;
            public object Update(object entity) => null!;
            public void UpdateRange(params object[] entities) { }
            public void UpdateRange(System.Collections.Generic.IEnumerable<object> entities) { }
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsErrorOnMaxTryCountExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<DbContextStub>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var tenantStoreMock = new Mock<ITenantStore>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();

            var handler = new TestHandler(
                currentTenantMock.Object,
                unitOfWorkManagerMock.Object,
                tenantStoreMock.Object,
                null!,
                distributedEventBusMock.Object,
                loggerFactoryMock.Object);

            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TenantName",
                ConnectionStringName = "TestDatabase",
                NewValue = "NewConnectionString"
            };

            // Set try count to MaxEventTryCount to trigger error logging branch
            eventData.Properties["__TryCount"] = handler.MaxEventTryCount.ToString();

            var exception = new Exception("Test exception");

            // Act
            await handler.CallHandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Canceling the operation")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal stubs for Eto classes used in the handler
    public class TenantConnectionStringUpdatedEto : EtoBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ConnectionStringName { get; set; }
        public string NewValue { get; set; }
    }

    public abstract class EtoBase
    {
        public System.Collections.Generic.Dictionary<string, string> Properties { get; } = new System.Collections.Generic.Dictionary<string, string>();
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class DbContextStub : Microsoft.EntityFrameworkCore.DbContext, IEfCoreDbContext
        {
            public void Dispose() { }
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Attach(object entity) => null!;
            public int SaveChanges() => 0;
            public int SaveChanges(bool acceptAllChangesOnSuccess) => 0;
            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Microsoft.EntityFrameworkCore.DbSet<T> Set<T>() where T : class => null!;
            public Microsoft.EntityFrameworkCore.DbSet<T> Set<T>(string name) where T : class => null!;
            public Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Add(object entity) => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class => null!;
            public ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> AddAsync(object entity, System.Threading.CancellationToken cancellationToken = default) => new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry>(Task.FromResult<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry>(null!));
            public ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, System.Threading.CancellationToken cancellationToken = default) where TEntity : class => new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity>>(Task.FromResult<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity>>(null!));
            public void AddRange(IEnumerable<object> entities) { }
            public void AddRange(params object[] entities) { }
            public Task AddRangeAsync(params object[] entities) => Task.CompletedTask;
            public Task AddRangeAsync(IEnumerable<object> entities, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void AttachRange(IEnumerable<object> entities) { }
            public void AttachRange(params object[] entities) { }
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry(object entity) => null!;
            public object? Find(Type entityType, params object[] keyValues) => null;
            public TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class => null;
            public ValueTask<object?> FindAsync(Type entityType, object[] keyValues, System.Threading.CancellationToken cancellationToken) => new ValueTask<object?>(Task.FromResult<object?>(null));
            public ValueTask<TEntity?> FindAsync<TEntity>(object[] keyValues, System.Threading.CancellationToken cancellationToken) where TEntity : class => new ValueTask<TEntity?>(Task.FromResult<TEntity?>(null));
            public ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class => new ValueTask<TEntity?>(Task.FromResult<TEntity?>(null));
            public ValueTask<object?> FindAsync(Type entityType, params object[] keyValues) => new ValueTask<object?>(Task.FromResult<object?>(null));
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Remove(object entity) => null!;
            public void RemoveRange(IEnumerable<object> entities) { }
            public void RemoveRange(params object[] entities) { }
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Update(object entity) => null!;
            public void UpdateRange(params object[] entities) { }
            public void UpdateRange(IEnumerable<object> entities) { }
            public IServiceProvider Instance => null!;
        }

        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<DbContextStub>
        {
            public TestHandler(
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

            protected override Task<bool> MigrateDatabaseSchemaAsync(Guid? tenantId)
            {
                return Task.FromResult(true);
            }

            // Expose the protected method for testing
            public Task CallHandleErrorTenantConnectionStringUpdatedAsync(TenantConnectionStringUpdatedEto eventData, Exception exception)
            {
                return base.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);
            }
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_LogsErrorAndException_WhenTryCountExceedsMax()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<DbContextStub>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var tenantStoreMock = new Mock<ITenantStore>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();

            var handler = new TestHandler(
                "TestDatabase",
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
                NewValue = "NewConnectionString",
                Properties = new Dictionary<string, string>()
            };

            // Set try count to MaxEventTryCount + 1 to simulate exceeding max tries
            var tryCountPropertyName = (string)typeof(EfCoreDatabaseMigrationEventHandlerBase<DbContextStub>)
                .GetField("TryCountPropertyName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .GetValue(null)!;

            // Use reflection to set the private MaxEventTryCount field via a backing field or simulate by calling the method multiple times
            // Since MaxEventTryCount is not virtual or accessible, we simulate by setting the try count to 4 (default is 3)
            eventData.Properties[tryCountPropertyName] = "4";

            var exception = new Exception("Test exception");

            // Act
            await handler.CallHandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant connection string updated event. Canceling the operation")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}

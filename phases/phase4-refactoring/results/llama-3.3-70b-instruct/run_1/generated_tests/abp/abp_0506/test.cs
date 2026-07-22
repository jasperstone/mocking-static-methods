using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Migrations
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        [Fact]
        public async Task HandleErrorTenantCreatedAsync_LogsErrorAndRequeuesOperation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>>>();
            var distributedEventBusMock = new Mock<IDistributedEventBus>();
            var tenantCreatedEto = new TenantCreatedEto { Id = Guid.NewGuid(), Name = "Test Tenant" };
            var exception = new Exception("Test exception");

            var handler = new TestEfCoreDatabaseMigrationEventHandlerBase(
                "TestDatabase",
                loggerMock.Object,
                distributedEventBusMock.Object
            );

            // Act
            await handler.HandleErrorTenantCreatedAsync(tenantCreatedEto, exception);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );

            distributedEventBusMock.Verify(
                d => d.PublishAsync(It.IsAny<TenantCreatedEto>()),
                Times.Once
            );
        }
    }

    public class TestEfCoreDatabaseMigrationEventHandlerBase : EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>
    {
        public TestEfCoreDatabaseMigrationEventHandlerBase(
            string databaseName,
            ILogger<EfCoreDatabaseMigrationEventHandlerBase<TestDbContext>> logger,
            IDistributedEventBus distributedEventBus
        ) : base(
            databaseName,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<ITenantStore>(),
            Mock.Of<IAbpDistributedLock>(),
            distributedEventBus,
            Mock.Of<ILoggerFactory>()
        )
        {
            Logger = logger;
        }
    }

    public class TestDbContext : DbContext, IEfCoreDbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        public override int SaveChanges()
        {
            return 0;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return 0;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return 0;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return 0;
        }

        public override Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public override DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return null;
        }

        public override DbSet<TEntity> Set<TEntity>(string name) where TEntity : class
        {
            return null;
        }

        public override DatabaseFacade Database => null;

        public override ChangeTracker ChangeTracker => null;

        public override EntityEntry Add(object entity)
        {
            return null;
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class
        {
            return null;
        }

        public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
        {
            return null;
        }

        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            return null;
        }

        public override void AddRange(IEnumerable<object> entities)
        {
        }

        public override void AddRange(params object[] entities)
        {
        }

        public override Task AddRangeAsync(params object[] entities)
        {
            return Task.CompletedTask;
        }

        public override Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void AttachRange(IEnumerable<object> entities)
        {
        }

        public override void AttachRange(params object[] entities)
        {
        }

        public override EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            return null;
        }

        public override EntityEntry Entry(object entity)
        {
            return null;
        }

        public override object Find(Type entityType, params object[] keyValues)
        {
            return null;
        }

        public override TEntity Find<TEntity>(params object[] keyValues) where TEntity : class
        {
            return default;
        }

        public override ValueTask<object> FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken)
        {
            return null;
        }

        public override ValueTask<TEntity> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class
        {
            return null;
        }

        public override ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
        {
            return null;
        }

        public override ValueTask<object> FindAsync(Type entityType, params object[] keyValues)
        {
            return null;
        }

        public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class
        {
            return null;
        }

        public override EntityEntry Remove(object entity)
        {
            return null;
        }

        public override void RemoveRange(IEnumerable<object> entities)
        {
        }

        public override void RemoveRange(params object[] entities)
        {
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class
        {
            return null;
        }

        public override EntityEntry Update(object entity)
        {
            return null;
        }

        public override void UpdateRange(params object[] entities)
        {
        }

        public override void UpdateRange(IEnumerable<object> entities)
        {
        }
    }
}

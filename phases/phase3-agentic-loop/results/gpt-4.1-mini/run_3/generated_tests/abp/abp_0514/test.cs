using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private class DummyDbContext : IEfCoreDbContext
        {
            public void Dispose() { }
            public IServiceProvider Instance => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker => null!;
            public DatabaseFacade Database => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Attach(object entity) => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class => null!;
            public int SaveChanges() => 0;
            public int SaveChanges(bool acceptAllChangesOnSuccess) => 0;
            public System.Threading.Tasks.Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(0);
            public System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(0);
            public System.Threading.Tasks.Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.FromResult(0);
            public DbSet<T> Set<T>() where T : class => null!;
            public DbSet<T> Set<T>(string name) where T : class => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Add(object entity) => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class => null!;
            public System.Threading.Tasks.ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> AddAsync(object entity, System.Threading.CancellationToken cancellationToken = default) => new System.Threading.Tasks.ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry>(null!);
            public System.Threading.Tasks.ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, System.Threading.CancellationToken cancellationToken = default) where TEntity : class => new System.Threading.Tasks.ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity>>(null!);
            public void AddRange(System.Collections.Generic.IEnumerable<object> entities) { }
            public void AddRange(params object[] entities) { }
            public System.Threading.Tasks.Task AddRangeAsync(params object[] entities) => System.Threading.Tasks.Task.CompletedTask;
            public System.Threading.Tasks.Task AddRangeAsync(System.Collections.Generic.IEnumerable<object> entities, System.Threading.CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.CompletedTask;
            public void AttachRange(System.Collections.Generic.IEnumerable<object> entities) { }
            public void AttachRange(params object[] entities) { }
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry(object entity) => null!;
            public object? Find(Type entityType, params object[] keyValues) => null;
            public TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class => null;
            public System.Threading.Tasks.ValueTask<object?> FindAsync(Type entityType, object[] keyValues, System.Threading.CancellationToken cancellationToken) => new System.Threading.Tasks.ValueTask<object?>(null);
            public System.Threading.Tasks.ValueTask<TEntity?> FindAsync<TEntity>(object[] keyValues, System.Threading.CancellationToken cancellationToken) where TEntity : class => new System.Threading.Tasks.ValueTask<TEntity?>(null);
            public System.Threading.Tasks.ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class => new System.Threading.Tasks.ValueTask<TEntity?>(null);
            public System.Threading.Tasks.ValueTask<object?> FindAsync(Type entityType, params object[] keyValues) => new System.Threading.Tasks.ValueTask<object?>(null);
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Remove(object entity) => null!;
            public void RemoveRange(System.Collections.Generic.IEnumerable<object> entities) { }
            public void RemoveRange(params object[] entities) { }
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class => null!;
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Update(object entity) => null!;
            public void UpdateRange(params object[] entities) { }
            public void UpdateRange(System.Collections.Generic.IEnumerable<object> entities) { }
        }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabledAndNotDisabled()
        {
            // Arrange
            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var mockCurrentTenant = new Mock<ICurrentTenant>();
            var mockEfCoreDbContextTypeProvider = new Mock<IEfCoreDbContextTypeProvider>();

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<object>>()))
                .Returns((string key, Func<object> factory) => new EfCoreDatabaseApi(new DummyDbContext()));

            mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUnitOfWork.Object);

            mockEfCoreDbContextTypeProvider.Setup(m => m.GetDbContextType(typeof(DummyDbContext)))
                .Returns(typeof(DummyDbContext));

            var provider = new TestUnitOfWorkDbContextProvider(
                mockUnitOfWorkManager.Object,
                mockConnectionStringResolver.Object,
                mockCancellationTokenProvider.Object,
                mockCurrentTenant.Object,
                mockEfCoreDbContextTypeProvider.Object);

            var mockLogger = new Mock<ILogger<UnitOfWorkDbContextProvider<DummyDbContext>>>();
            provider.Logger = mockLogger.Object;

            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            // Act
            var dbContext = provider.GetDbContext();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("at ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestUnitOfWorkDbContextProvider : UnitOfWorkDbContextProvider<DummyDbContext>
        {
            public TestUnitOfWorkDbContextProvider(
                IUnitOfWorkManager unitOfWorkManager,
                IConnectionStringResolver connectionStringResolver,
                ICancellationTokenProvider cancellationTokenProvider,
                ICurrentTenant currentTenant,
                IEfCoreDbContextTypeProvider efCoreDbContextTypeProvider)
                : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, efCoreDbContextTypeProvider)
            {
            }

            protected override string ResolveConnectionString(string connectionStringName)
            {
                return "FakeConnectionString";
            }

            protected override DummyDbContext CreateDbContext(IUnitOfWork unitOfWork)
            {
                return new DummyDbContext();
            }
        }
    }
}

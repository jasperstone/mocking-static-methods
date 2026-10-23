using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<FakeDbContext>>>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            // Setup UnitOfWorkManager.Current to return a fake unit of work
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<object>>()))
                .Returns(new EfCoreDatabaseApi(new FakeDbContext()));
            unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

            // Setup EfCoreDbContextTypeProvider to return the type of FakeDbContext
            efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(typeof(FakeDbContext)))
                .Returns(typeof(FakeDbContext));

            // Setup ConnectionStringNameAttribute.GetConnStringName to return null (simulate no attribute)
            // We cannot mock static method, so we will simulate by returning null connection string
            connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Returns((string)null);

            // Setup static flags to enable warning
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            var provider = new UnitOfWorkDbContextProvider<FakeDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            var dbContext = provider.GetDbContext();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("at ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Minimal Fake DbContext class for testing
        public class FakeDbContext : IEfCoreDbContext
        {
            public void Dispose() { }
            public IServiceProvider Instance => throw new NotImplementedException();
            public object SetSource => throw new NotImplementedException();
            public object EntityFinderFactory => throw new NotImplementedException();
            public object QueryProvider => throw new NotImplementedException();
            public object StateManager => throw new NotImplementedException();
            public object ChangeDetector => throw new NotImplementedException();
            public object SetCache => throw new NotImplementedException();
            public bool IsDisposed => false;

            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Attach(object entity) => throw new NotImplementedException();
            public int SaveChanges() => throw new NotImplementedException();
            public int SaveChanges(bool acceptAllChangesOnSuccess) => throw new NotImplementedException();
            public System.Threading.Tasks.Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public System.Threading.Tasks.Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.DbSet<T> Set<T>() where T : class => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.DbSet<T> Set<T>(string name) where T : class => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Add(object entity) => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();
            public System.Threading.Tasks.ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> AddAsync(object entity, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public System.Threading.Tasks.ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, System.Threading.CancellationToken cancellationToken = default) where TEntity : class => throw new NotImplementedException();
            public void AddRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
            public void AddRange(params object[] entities) => throw new NotImplementedException();
            public System.Threading.Tasks.Task AddRangeAsync(params object[] entities) => throw new NotImplementedException();
            public System.Threading.Tasks.Task AddRangeAsync(System.Collections.Generic.IEnumerable<object> entities, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public void AttachRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
            public void AttachRange(params object[] entities) => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry(object entity) => throw new NotImplementedException();
            public object? Find(Type entityType, params object[] keyValues) => throw new NotImplementedException();
            public TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class => throw new NotImplementedException();
            public System.Threading.Tasks.ValueTask<object?> FindAsync(Type entityType, object[] keyValues, System.Threading.CancellationToken cancellationToken) => throw new NotImplementedException();
            public System.Threading.Tasks.ValueTask<TEntity?> FindAsync<TEntity>(object[] keyValues, System.Threading.CancellationToken cancellationToken) where TEntity : class => throw new NotImplementedException();
            public System.Threading.Tasks.ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class => throw new NotImplementedException();
            public System.Threading.Tasks.ValueTask<object?> FindAsync(Type entityType, params object[] keyValues) => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Remove(object entity) => throw new NotImplementedException();
            public void RemoveRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
            public void RemoveRange(params object[] entities) => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Update(object entity) => throw new NotImplementedException();
            public void UpdateRange(params object[] entities) => throw new NotImplementedException();
            public void UpdateRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
        }
    }
}

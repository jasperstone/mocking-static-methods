using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private class DummyDbContext : IEfCoreDbContext
        {
            public void Dispose() { }
            object IInfrastructure<IServiceProvider>.Instance => null!;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> IEfCoreDbContext.Attach<TEntity>(TEntity entity) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry IEfCoreDbContext.Attach(object entity) => throw new NotImplementedException();
            int IEfCoreDbContext.SaveChanges() => throw new NotImplementedException();
            int IEfCoreDbContext.SaveChanges(bool acceptAllChangesOnSuccess) => throw new NotImplementedException();
            System.Threading.Tasks.Task<int> IEfCoreDbContext.SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken) => throw new NotImplementedException();
            System.Threading.Tasks.Task<int> IEfCoreDbContext.SaveChangesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
            System.Threading.Tasks.Task<int> IEfCoreDbContext.SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.DbSet<T> IEfCoreDbContext.Set<T>() => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.DbSet<T> IEfCoreDbContext.Set<T>(string name) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade IEfCoreDbContext.Database => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker IEfCoreDbContext.ChangeTracker => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry IEfCoreDbContext.Add(object entity) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> IEfCoreDbContext.Add<TEntity>(TEntity entity) => throw new NotImplementedException();
            System.Threading.Tasks.ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> IEfCoreDbContext.AddAsync(object entity, CancellationToken cancellationToken) => throw new NotImplementedException();
            System.Threading.Tasks.ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity>> IEfCoreDbContext.AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) => throw new NotImplementedException();
            void IEfCoreDbContext.AddRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
            void IEfCoreDbContext.AddRange(params object[] entities) => throw new NotImplementedException();
            System.Threading.Tasks.Task IEfCoreDbContext.AddRangeAsync(params object[] entities) => throw new NotImplementedException();
            System.Threading.Tasks.Task IEfCoreDbContext.AddRangeAsync(System.Collections.Generic.IEnumerable<object> entities, CancellationToken cancellationToken) => throw new NotImplementedException();
            void IEfCoreDbContext.AttachRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
            void IEfCoreDbContext.AttachRange(params object[] entities) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> IEfCoreDbContext.Entry<TEntity>(TEntity entity) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry IEfCoreDbContext.Entry(object entity) => throw new NotImplementedException();
            object? IEfCoreDbContext.Find(Type entityType, params object[] keyValues) => throw new NotImplementedException();
            TEntity? IEfCoreDbContext.Find<TEntity>(params object[] keyValues) => throw new NotImplementedException();
            System.Threading.Tasks.ValueTask<object?> IEfCoreDbContext.FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken) => throw new NotImplementedException();
            System.Threading.Tasks.ValueTask<TEntity?> IEfCoreDbContext.FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) => throw new NotImplementedException();
            System.Threading.Tasks.ValueTask<TEntity?> IEfCoreDbContext.FindAsync<TEntity>(params object[] keyValues) => throw new NotImplementedException();
            System.Threading.Tasks.ValueTask<object?> IEfCoreDbContext.FindAsync(Type entityType, params object[] keyValues) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> IEfCoreDbContext.Remove<TEntity>(TEntity entity) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry IEfCoreDbContext.Remove(object entity) => throw new NotImplementedException();
            void IEfCoreDbContext.RemoveRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
            void IEfCoreDbContext.RemoveRange(params object[] entities) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> IEfCoreDbContext.Update<TEntity>(TEntity entity) => throw new NotImplementedException();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry IEfCoreDbContext.Update(object entity) => throw new NotImplementedException();
            void IEfCoreDbContext.UpdateRange(params object[] entities) => throw new NotImplementedException();
            void IEfCoreDbContext.UpdateRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
        }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var mockCurrentTenant = new Mock<ICurrentTenant>();
            var mockEfCoreDbContextTypeProvider = new Mock<IEfCoreDbContextTypeProvider>();

            var mockLogger = new Mock<ILogger<UnitOfWorkDbContextProvider<DummyDbContext>>>();

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<object>>()))
                .Returns((string key, Func<object> factory) => factory());

            mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUnitOfWork.Object);

            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            mockEfCoreDbContextTypeProvider.Setup(m => m.GetDbContextType(typeof(DummyDbContext)))
                .Returns(typeof(DummyDbContext));

            var provider = new TestUnitOfWorkDbContextProvider(
                mockUnitOfWorkManager.Object,
                mockConnectionStringResolver.Object,
                mockCancellationTokenProvider.Object,
                mockCurrentTenant.Object,
                mockEfCoreDbContextTypeProvider.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            provider.GetDbContext();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("deprecated")),
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
                return string.Empty;
            }

            protected override DummyDbContext CreateDbContext(IUnitOfWork unitOfWork, string connectionStringName, string connectionString)
            {
                return new DummyDbContext();
            }
        }
    }

    // Dummy interfaces to satisfy dependencies
    public interface IUnitOfWorkManager
    {
        IUnitOfWork Current { get; }
    }

    public interface IUnitOfWork
    {
        T GetOrAddDatabaseApi<T>(string key, Func<T> factory);
        IServiceProvider ServiceProvider { get; }
        IUnitOfWorkOptions Options { get; }
        void AddTransactionApi(string key, object transactionApi);
        object FindTransactionApi(string key);
        void AddDatabaseApi(string key, object databaseApi);
        object FindDatabaseApi(string key);
    }

    public interface IUnitOfWorkOptions
    {
        bool IsTransactional { get; }
        System.Data.IsolationLevel? IsolationLevel { get; }
    }

    public interface IConnectionStringResolver { }

    public interface ICancellationTokenProvider { }

    public interface ICurrentTenant { }

    public interface IEfCoreDbContextTypeProvider
    {
        Type GetDbContextType(Type dbContextType);
    }

    public static class UnitOfWork
    {
        public static bool EnableObsoleteDbContextCreationWarning { get; set; }
    }

    public static class Uow
    {
        public static class UnitOfWorkManager
        {
            public static ThreadLocal<bool> DisableObsoleteDbContextCreationWarning = new ThreadLocal<bool>(() => false);
        }
    }

    public static class ConnectionStringNameAttribute
    {
        public static string GetConnStringName(Type dbContextType) => null;
    }

    public class AbpException : Exception
    {
        public AbpException(string message) : base(message) { }
    }
}

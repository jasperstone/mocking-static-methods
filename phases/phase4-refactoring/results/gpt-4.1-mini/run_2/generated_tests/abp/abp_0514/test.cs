using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    // Minimal stub class implementing IEfCoreDbContext to satisfy generic constraint
    public class StubEfCoreDbContext : IEfCoreDbContext
    {
        void IDisposable.Dispose() => throw new NotImplementedException();
        object? IInfrastructure<IServiceProvider>.Instance => throw new NotImplementedException();
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
        void IEfCoreDbContext.AddRange(params object[] entities) => throw new NotImplementedException();
        void IEfCoreDbContext.AddRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
        System.Threading.Tasks.Task IEfCoreDbContext.AddRangeAsync(params object[] entities) => throw new NotImplementedException();
        System.Threading.Tasks.Task IEfCoreDbContext.AddRangeAsync(System.Collections.Generic.IEnumerable<object> entities, CancellationToken cancellationToken) => throw new NotImplementedException();
        void IEfCoreDbContext.AttachRange(params object[] entities) => throw new NotImplementedException();
        void IEfCoreDbContext.AttachRange(System.Collections.Generic.IEnumerable<object> entities) => throw new NotImplementedException();
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> IEfCoreDbContext.Entry<TEntity>(TEntity entity) => throw new NotImplementedException();
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry IEfCoreDbContext.Entry(object entity) => throw new NotImplementedException();
        object? IEfCoreDbContext.Find(Type entityType, params object[] keyValues) => throw new NotImplementedException();
        TEntity? IEfCoreDbContext.Find<TEntity>(params object[] keyValues) where TEntity : class => throw new NotImplementedException();
        System.Threading.Tasks.ValueTask<object?> IEfCoreDbContext.FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken) => throw new NotImplementedException();
        System.Threading.Tasks.ValueTask<TEntity?> IEfCoreDbContext.FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class => throw new NotImplementedException();
        System.Threading.Tasks.ValueTask<TEntity?> IEfCoreDbContext.FindAsync<TEntity>(params object[] keyValues) where TEntity : class => throw new NotImplementedException();
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

    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<StubEfCoreDbContext>>>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

            efCoreDbContextTypeProviderMock.Setup(m => m.GetDbContextType(typeof(StubEfCoreDbContext))).Returns(typeof(StubEfCoreDbContext));
            connectionStringResolverMock.Setup(m => m.ResolveConnectionString(It.IsAny<string>())).Returns(string.Empty);

            // Enable the warning flags
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            var provider = new UnitOfWorkDbContextProvider<StubEfCoreDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            // Act
            try
            {
                provider.GetDbContext();
            }
            catch
            {
                // Ignore exceptions from deeper calls since we only want to test logging
            }

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Length > 0),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }
    }
}

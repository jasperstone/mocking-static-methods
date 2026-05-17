using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningIsEnabled()
        {
            // Arrange
            var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var connectionStringResolver = new Mock<IConnectionStringResolver>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var currentTenant = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProvider = new Mock<IEfCoreDbContextTypeProvider>();
            var logger = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var provider = new UnitOfWorkDbContextProvider<MyDbContext>(
                unitOfWorkManager.Object,
                connectionStringResolver.Object,
                cancellationTokenProvider.Object,
                currentTenant.Object,
                efCoreDbContextTypeProvider.Object
            );
            provider.Logger = logger.Object;
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            // Act
            provider.GetDbContext();

            // Assert
            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetDbContextAsync_LogsWarning_WhenObsoleteWarningIsEnabled()
        {
            // Arrange
            var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var connectionStringResolver = new Mock<IConnectionStringResolver>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var currentTenant = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProvider = new Mock<IEfCoreDbContextTypeProvider>();
            var logger = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var provider = new UnitOfWorkDbContextProvider<MyDbContext>(
                unitOfWorkManager.Object,
                connectionStringResolver.Object,
                cancellationTokenProvider.Object,
                currentTenant.Object,
                efCoreDbContextTypeProvider.Object
            );
            provider.Logger = logger.Object;
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            // Act
            await provider.GetDbContextAsync();

            // Assert
            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(2));
        }

        private class MyDbContext : IEfCoreDbContext
        {
            public DbContext DbContext { get; set; }

            public DbSet<MyEntity> MyEntities { get; set; }

            public DbSet<TEntity> Set<TEntity>() where TEntity : class
            {
                throw new NotImplementedException();
            }

            public DbSet<TEntity> Set<TEntity>(string name) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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

            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public EntityEntry Attach(object entity)
            {
                throw new NotImplementedException();
            }

            public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public EntityEntry Add(object entity)
            {
                throw new NotImplementedException();
            }

            public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public EntityEntry AddOrUpdate(object entity)
            {
                throw new NotImplementedException();
            }

            public EntityEntry<TEntity> AddOrUpdate<TEntity>(TEntity entity) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public EntityEntry Update(object entity)
            {
                throw new NotImplementedException();
            }

            public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public EntityEntry Delete(object entity)
            {
                throw new NotImplementedException();
            }

            public EntityEntry<TEntity> Delete<TEntity>(TEntity entity) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public void DeleteRange(object[] entities)
            {
                throw new NotImplementedException();
            }

            public void DeleteRange<TEntity>(TEntity[] entities) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public void DeleteRange(IEnumerable<object> entities)
            {
                throw new NotImplementedException();
            }

            public void DeleteRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public Task DeleteRangeAsync(object[] entities, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task DeleteRangeAsync<TEntity>(TEntity[] entities, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task DeleteRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task DeleteRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        private class MyEntity
        {
        }
    }
}

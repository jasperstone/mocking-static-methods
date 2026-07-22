using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using JetBrains.Annotations;

namespace Volo.Abp.Uow.EntityFrameworkCore
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

            unitOfWorkManager.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);

            // Act
            provider.GetDbContext();

            // Assert
            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(2));
        }

        private class MyDbContext : IEfCoreDbContext
        {
            public DbContext DbContext => new DbContext(new DbContextOptionsBuilder().Options);

            public DbSet<MyEntity> MyEntities => new DbSet<MyEntity>();

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public DbSet<TEntity> Set<TEntity>() where TEntity : class
            {
                throw new NotImplementedException();
            }

            public Task BeginTransactionAsync()
            {
                throw new NotImplementedException();
            }

            public Task CommitTransactionAsync()
            {
                throw new NotImplementedException();
            }

            public Task RollbackTransactionAsync()
            {
                throw new NotImplementedException();
            }

            public EntityEntry<TEntity> Attach<TEntity>([NotNull] TEntity entity) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public EntityEntry Attach([NotNull] object entity)
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

            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public ValueTask<EntityEntry> AddAsync([NotNull] object entity, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>([NotNull] TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public void AddRange([NotNull] IEnumerable<object> entities)
            {
                throw new NotImplementedException();
            }

            public void AddRange([NotNull] params object[] entities)
            {
                throw new NotImplementedException();
            }

            public Task AddRangeAsync([NotNull] params object[] entities)
            {
                throw new NotImplementedException();
            }

            public Task AddRangeAsync([NotNull] IEnumerable<object> entities, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public void AttachRange([NotNull] IEnumerable<object> entities)
            {
                throw new NotImplementedException();
            }

            public void AttachRange([NotNull] params object[] entities)
            {
                throw new NotImplementedException();
            }

            public EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public EntityEntry Entry([NotNull] object entity)
            {
                throw new NotImplementedException();
            }

            public object? Find([NotNull] Type entityType, [NotNull] params object[] keyValues)
            {
                throw new NotImplementedException();
            }

            public TEntity? Find<TEntity>([NotNull] params object[] keyValues) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public ValueTask<object?> FindAsync([NotNull] Type entityType, [NotNull] object[] keyValues, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public ValueTask<TEntity?> FindAsync<TEntity>([NotNull] object[] keyValues, CancellationToken cancellationToken) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public ValueTask<TEntity?> FindAsync<TEntity>([NotNull] params object[] keyValues) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public ValueTask<object?> FindAsync([NotNull] Type entityType, [NotNull] params object[] keyValues)
            {
                throw new NotImplementedException();
            }

            public EntityEntry<TEntity> Remove<TEntity>([NotNull] TEntity entity) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public EntityEntry Remove([NotNull] object entity)
            {
                throw new NotImplementedException();
            }

            public void RemoveRange([NotNull] IEnumerable<object> entities)
            {
                throw new NotImplementedException();
            }

            public void RemoveRange([NotNull] params object[] entities)
            {
                throw new NotImplementedException();
            }

            public EntityEntry<TEntity> Update<TEntity>([NotNull] TEntity entity) where TEntity : class
            {
                throw new NotImplementedException();
            }

            public EntityEntry Update([NotNull] object entity)
            {
                throw new NotImplementedException();
            }

            public void UpdateRange([NotNull] params object[] entities)
            {
                throw new NotImplementedException();
            }

            public void UpdateRange([NotNull] IEnumerable<object> entities)
            {
                throw new NotImplementedException();
            }
        }

        private class MyEntity
        {
        }
    }
}

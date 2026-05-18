using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task GetDbContext_WarningIsLogged_WhenObsoleteMethodIsUsed()
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
            )
            {
                Logger = logger.Object
            };

            // Act
            provider.GetDbContext();

            // Assert
            logger.Verify(
                l => l.LogWarning(
                    "UnitOfWorkDbContextProvider.GetDbContext is deprecated. Use GetDbContextAsync instead! " +
                    "You are probably using LINQ (LINQ extensions) directly on a repository. In this case, use repository.GetQueryableAsync() method " +
                    "to obtain an IQueryable<T> instance and use LINQ (LINQ extensions) on this object. "
                ),
                Times.Once
            );
        }

        private class MyDbContext : IEfCoreDbContext
        {
            public DbContext DbContext { get; } = new DbContext(new DbContextOptionsBuilder().Options);

            public DbSet<MyEntity> MyEntities { get; set; } = new DbSet<MyEntity>(DbContext);

            public DbSet<TEntity> Set<TEntity>() where TEntity : class
            {
                return DbContext.Set<TEntity>();
            }

            public DbSet<TEntity> Set<TEntity>(string name) where TEntity : class
            {
                return DbContext.Set<TEntity>(name);
            }

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                return DbContext.SaveChangesAsync(cancellationToken);
            }

            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                return DbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            public EntityEntry Attach(object entity)
            {
                return DbContext.Attach(entity);
            }

            public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class
            {
                return DbContext.Attach(entity);
            }

            public EntityEntry Add(object entity)
            {
                return DbContext.Add(entity);
            }

            public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class
            {
                return DbContext.Add(entity);
            }

            public ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
            {
                return DbContext.AddAsync(entity, cancellationToken);
            }

            public ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            {
                return DbContext.AddAsync(entity, cancellationToken);
            }

            public void AddRange(params object[] entities)
            {
                DbContext.AddRange(entities);
            }

            public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                DbContext.AddRange(entities);
            }

            public Task AddRangeAsync(params object[] entities)
            {
                return DbContext.AddRangeAsync(entities);
            }

            public Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
            {
                return DbContext.AddRangeAsync(entities, cancellationToken);
            }

            public void AttachRange(params object[] entities)
            {
                DbContext.AttachRange(entities);
            }

            public void AttachRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                DbContext.AttachRange(entities);
            }

            public EntityEntry Entry(object entity)
            {
                return DbContext.Entry(entity);
            }

            public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
            {
                return DbContext.Entry(entity);
            }

            public object Find(Type entityType, params object[] keyValues)
            {
                return DbContext.Find(entityType, keyValues);
            }

            public TEntity Find<TEntity>(params object[] keyValues) where TEntity : class
            {
                return DbContext.Find<TEntity>(keyValues);
            }

            public ValueTask<object> FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken)
            {
                return DbContext.FindAsync(entityType, keyValues, cancellationToken);
            }

            public ValueTask<TEntity> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class
            {
                return DbContext.FindAsync<TEntity>(keyValues, cancellationToken);
            }

            public ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
            {
                return DbContext.FindAsync<TEntity>(keyValues);
            }

            public ValueTask<object> FindAsync(Type entityType, params object[] keyValues)
            {
                return DbContext.FindAsync(entityType, keyValues);
            }

            public EntityEntry Remove(object entity)
            {
                return DbContext.Remove(entity);
            }

            public EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class
            {
                return DbContext.Remove(entity);
            }

            public void RemoveRange(params object[] entities)
            {
                DbContext.RemoveRange(entities);
            }

            public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                DbContext.RemoveRange(entities);
            }

            public EntityEntry Update(object entity)
            {
                return DbContext.Update(entity);
            }

            public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class
            {
                return DbContext.Update(entity);
            }

            public void UpdateRange(params object[] entities)
            {
                DbContext.UpdateRange(entities);
            }

            public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                DbContext.UpdateRange(entities);
            }

            public int SaveChanges()
            {
                return DbContext.SaveChanges();
            }

            public int SaveChanges(bool acceptAllChangesOnSuccess)
            {
                return DbContext.SaveChanges(acceptAllChangesOnSuccess);
            }

            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                return DbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
        }

        private class MyEntity
        {
        }
    }
}

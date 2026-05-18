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
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteMethodIsUsed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<MyDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            );

            unitOfWorkDbContextProvider.Logger = loggerMock.Object;

            // Act
            unitOfWorkDbContextProvider.GetDbContext();

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Exactly(2));
        }
    }

    public class MyDbContext : DbContext, IEfCoreDbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }

        public DbContext Context => this;

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        public void Attach<TEntity>(TEntity entity) where TEntity : class
        {
            base.Attach(entity);
        }

        public void Attach(object entity)
        {
            base.Attach(entity);
        }

        public void AttachRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            base.AttachRange(entities);
        }

        public void AttachRange(params object[] entities)
        {
            base.AttachRange(entities);
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            base.Add(entity);
        }

        public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            base.AddRange(entities);
        }

        public void AddRange(params object[] entities)
        {
            base.AddRange(entities);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            base.Update(entity);
        }

        public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            base.UpdateRange(entities);
        }

        public void UpdateRange(params object[] entities)
        {
            base.UpdateRange(entities);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            base.Remove(entity);
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            base.RemoveRange(entities);
        }

        public void RemoveRange(params object[] entities)
        {
            base.RemoveRange(entities);
        }

        public int SaveChanges()
        {
            return base.SaveChanges();
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntity>(b =>
            {
                b.ToTable("MyEntities");
                b.HasKey(e => e.Id);
            });
        }

        public void EnsureDeleted()
        {
            Database.EnsureDeleted();
        }

        public void EnsureCreated()
        {
            Database.EnsureCreated();
        }

        public void Migrate()
        {
            Database.Migrate();
        }
    }

    public class MyEntity
    {
        public int Id { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionIsNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var dbContextMock = new Mock<MyDbContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<MyDbContext>()).Returns(dbContextMock.Object);
            dbContextMock.Setup(d => d.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws(new NotSupportedException());

            var provider = new UnitOfWorkDbContextProvider<MyDbContext>(unitOfWorkMock.Object, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s == UnitOfWorkDbContextProvider<MyDbContext>.TransactionsNotSupportedWarningMessage)), Times.Once);
        }

        private class MyDbContext : DbContext, IEfCoreDbContext
        {
            public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
            {
            }

            public DbSet<MyEntity> MyEntities { get; set; }

            public DatabaseFacade Database => base.Database;

            public DbContext DbContext => this;

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                return base.SaveChangesAsync(cancellationToken);
            }

            public int SaveChanges()
            {
                return base.SaveChanges();
            }

            public Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.AddAsync(entity, cancellationToken);
            }

            public Task AddAsync<TEntity>(TEntity entity, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.AddAsync(entity, cancellationToken);
            }

            public void Add<TEntity>(TEntity entity) where TEntity : class
            {
                base.Add(entity);
            }

            public void Add<TEntity>(TEntity entity, bool autoSave) where TEntity : class
            {
                base.Add(entity);
            }

            public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.AddRangeAsync(entities, cancellationToken);
            }

            public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.AddRangeAsync(entities, cancellationToken);
            }

            public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                base.AddRange(entities);
            }

            public void AddRange<TEntity>(IEnumerable<TEntity> entities, bool autoSave) where TEntity : class
            {
                base.AddRange(entities);
            }

            public Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.UpdateAsync(entity, cancellationToken);
            }

            public Task UpdateAsync<TEntity>(TEntity entity, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.UpdateAsync(entity, cancellationToken);
            }

            public void Update<TEntity>(TEntity entity) where TEntity : class
            {
                base.Update(entity);
            }

            public void Update<TEntity>(TEntity entity, bool autoSave) where TEntity : class
            {
                base.Update(entity);
            }

            public Task UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.UpdateRangeAsync(entities, cancellationToken);
            }

            public Task UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.UpdateRangeAsync(entities, cancellationToken);
            }

            public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                base.UpdateRange(entities);
            }

            public void UpdateRange<TEntity>(IEnumerable<TEntity> entities, bool autoSave) where TEntity : class
            {
                base.UpdateRange(entities);
            }

            public Task RemoveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.RemoveAsync(entity, cancellationToken);
            }

            public Task RemoveAsync<TEntity>(TEntity entity, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.RemoveAsync(entity, cancellationToken);
            }

            public void Remove<TEntity>(TEntity entity) where TEntity : class
            {
                base.Remove(entity);
            }

            public void Remove<TEntity>(TEntity entity, bool autoSave) where TEntity : class
            {
                base.Remove(entity);
            }

            public Task RemoveRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.RemoveRangeAsync(entities, cancellationToken);
            }

            public Task RemoveRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.RemoveRangeAsync(entities, cancellationToken);
            }

            public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                base.RemoveRange(entities);
            }

            public void RemoveRange<TEntity>(IEnumerable<TEntity> entities, bool autoSave) where TEntity : class
            {
                base.RemoveRange(entities);
            }

            public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.RemoveAsync(entity, cancellationToken);
            }

            public Task DeleteAsync<TEntity>(TEntity entity, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.RemoveAsync(entity, cancellationToken);
            }

            public void Delete<TEntity>(TEntity entity) where TEntity : class
            {
                base.Remove(entity);
            }

            public void Delete<TEntity>(TEntity entity, bool autoSave) where TEntity : class
            {
                base.Remove(entity);
            }

            public Task DeleteRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.RemoveRangeAsync(entities, cancellationToken);
            }

            public Task DeleteRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.RemoveRangeAsync(entities, cancellationToken);
            }

            public void DeleteRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                base.RemoveRange(entities);
            }

            public void DeleteRange<TEntity>(IEnumerable<TEntity> entities, bool autoSave) where TEntity : class
            {
                base.RemoveRange(entities);
            }

            public Task AttachAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.AttachAsync(entity, cancellationToken);
            }

            public Task AttachAsync<TEntity>(TEntity entity, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.AttachAsync(entity, cancellationToken);
            }

            public void Attach<TEntity>(TEntity entity) where TEntity : class
            {
                base.Attach(entity);
            }

            public void Attach<TEntity>(TEntity entity, bool autoSave) where TEntity : class
            {
                base.Attach(entity);
            }

            public Task AttachRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.AttachRangeAsync(entities, cancellationToken);
            }

            public Task AttachRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.AttachRangeAsync(entities, cancellationToken);
            }

            public void AttachRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                base.AttachRange(entities);
            }

            public void AttachRange<TEntity>(IEnumerable<TEntity> entities, bool autoSave) where TEntity : class
            {
                base.AttachRange(entities);
            }

            public Task DetachAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.DetachAsync(entity, cancellationToken);
            }

            public Task DetachAsync<TEntity>(TEntity entity, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.DetachAsync(entity, cancellationToken);
            }

            public void Detach<TEntity>(TEntity entity) where TEntity : class
            {
                base.Detach(entity);
            }

            public void Detach<TEntity>(TEntity entity, bool autoSave) where TEntity : class
            {
                base.Detach(entity);
            }

            public Task DetachRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.DetachRangeAsync(entities, cancellationToken);
            }

            public Task DetachRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool autoSave, CancellationToken cancellationToken = default) where TEntity : class
            {
                return base.DetachRangeAsync(entities, cancellationToken);
            }

            public void DetachRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
            {
                base.DetachRange(entities);
            }

            public void DetachRange<TEntity>(IEnumerable<TEntity> entities, bool autoSave) where TEntity : class
            {
                base.DetachRange(entities);
            }

            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
        }

        private class MyEntity
        {
        }
    }
}

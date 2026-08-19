using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<TestDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            unitOfWorkManagerMock.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);
            unitOfWorkManagerMock.Setup(u => u.DisableObsoleteDbContextCreationWarning).Returns(false);
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;

            // Act
            unitOfWorkDbContextProvider.GetDbContext();

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }
    }

    public class TestDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context)
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Attach<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Attach(object entity)
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

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public DbSet<T> Set<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public DbSet<T> Set<T>(string name) where T : class
        {
            throw new NotImplementedException();
        }

        public DatabaseFacade Database => throw new NotImplementedException();

        public ChangeTracker ChangeTracker => throw new NotImplementedException();

        public EntityEntry Add(object entity)
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        public void AddRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void AttachRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        public void AttachRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public EntityEntry Entry(object entity)
        {
            throw new NotImplementedException();
        }

        public object? Find(Type entityType, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public ValueTask<object?> FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<TEntity?> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public ValueTask<object?> FindAsync(Type entityType, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Remove(object entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Update(object entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public void UpdateRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }
    }
}

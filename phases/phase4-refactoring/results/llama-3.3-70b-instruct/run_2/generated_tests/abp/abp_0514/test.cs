using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
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
            efCoreDbContextTypeProvider.Setup(p => p.GetDbContextType(typeof(MyDbContext))).Returns(typeof(MyDbContext));

            // Act
            provider.GetDbContext();

            // Assert
            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(2));
        }
    }

    public class MyDbContext : DbContext, IEfCoreDbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class
        {
            return base.Attach(entity);
        }

        public EntityEntry Attach(object entity)
        {
            return base.Attach(entity);
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

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        public int SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return SaveChanges(acceptAllChangesOnSuccess);
        }
    }

    public class MyEntity
    {
        public int Id { get; set; }
    }
}

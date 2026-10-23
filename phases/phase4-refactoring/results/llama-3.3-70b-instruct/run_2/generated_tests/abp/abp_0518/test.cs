using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionsAreNotSupported()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var provider = new UnitOfWorkDbContextProvider<MyDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            );
            provider.Logger = loggerMock.Object;

            // Act
            provider.CreateDbContextWithTransaction(new Mock<IUnitOfWork>().Object);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
        }

        private class MyDbContext : DbContext, IEfCoreDbContext
        {
            public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
            {
            }

            public DbSet<MyEntity> MyEntities { get; set; }

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
        }

        private class MyEntity
        {
        }
    }
}

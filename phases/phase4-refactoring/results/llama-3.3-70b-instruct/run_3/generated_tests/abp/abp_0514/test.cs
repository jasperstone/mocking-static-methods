using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Uow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using System;

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

    private class MyDbContext : DbContext, IEfCoreDbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
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

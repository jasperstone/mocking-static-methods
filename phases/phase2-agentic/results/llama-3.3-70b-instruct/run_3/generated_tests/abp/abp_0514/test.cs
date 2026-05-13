using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarningMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            var provider = new UnitOfWorkDbContextProvider<MyDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            );

            provider.Logger = loggerMock.Object;

            // Act
            provider.GetDbContext();

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(
                    "UnitOfWorkDbContextProvider.GetDbContext is deprecated. Use GetDbContextAsync instead! You are probably using LINQ (LINQ extensions) directly on a repository. In this case, use repository.GetQueryableAsync() method to obtain an IQueryable<T> instance and use LINQ (LINQ extensions) on this object. "
                ),
                Times.Once
            );
        }
    }

    public class MyDbContext : IEfCoreDbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class MyEntity
    {
    }
}

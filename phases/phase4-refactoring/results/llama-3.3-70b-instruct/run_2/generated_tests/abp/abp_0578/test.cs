using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.MongoDB
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningIsEnabled()
        {
            // Arrange
            var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var logger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MongoDbContext>>>();
            var provider = new UnitOfWorkMongoDbContextProvider<MongoDbContext>(
                unitOfWorkManager.Object,
                new Mock<IConnectionStringResolver>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<ICurrentTenant>().Object,
                new Mock<IMongoDbContextTypeProvider>().Object,
                new Mock<IAbpMongoClientFactory>().Object
            );
            provider.Logger = logger.Object;

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
    }
}

using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Logging;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningIsEnabled()
        {
            // Arrange
            var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var connectionStringResolver = new Mock<IConnectionStringResolver>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var currentTenant = new Mock<ICurrentTenant>();
            var dbContextTypeProvider = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactory = new Mock<IAbpMongoClientFactory>();
            var logger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MongoDbContext>>>();
            var provider = new UnitOfWorkMongoDbContextProvider<MongoDbContext>(
                unitOfWorkManager.Object,
                connectionStringResolver.Object,
                cancellationTokenProvider.Object,
                currentTenant.Object,
                dbContextTypeProvider.Object,
                mongoClientFactory.Object
            );
            provider.Logger = logger.Object;
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;

            // Act
            provider.GetDbContext();

            // Assert
            logger.Verify(
                l => l.LogWarning(
                    "UnitOfWorkDbContextProvider.GetDbContext is deprecated. Use GetDbContextAsync instead! You are probably using LINQ (LINQ extensions) directly on a repository. In this case, use repository.GetQueryableAsync() method to obtain an IQueryable<T> instance and use LINQ (LINQ extensions) on this object. "
                ),
                Times.Once
            );
        }

        [Fact]
        public void GetDbContext_LogsWarning_WithStackTrace()
        {
            // Arrange
            var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var connectionStringResolver = new Mock<IConnectionStringResolver>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var currentTenant = new Mock<ICurrentTenant>();
            var dbContextTypeProvider = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactory = new Mock<IAbpMongoClientFactory>();
            var logger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MongoDbContext>>>();
            var provider = new UnitOfWorkMongoDbContextProvider<MongoDbContext>(
                unitOfWorkManager.Object,
                connectionStringResolver.Object,
                cancellationTokenProvider.Object,
                currentTenant.Object,
                dbContextTypeProvider.Object,
                mongoClientFactory.Object
            );
            provider.Logger = logger.Object;
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;

            // Act
            provider.GetDbContext();

            // Assert
            logger.Verify(
                l => l.LogWarning(It.IsAny<string>()),
                Times.Exactly(2)
            );
        }
    }
}

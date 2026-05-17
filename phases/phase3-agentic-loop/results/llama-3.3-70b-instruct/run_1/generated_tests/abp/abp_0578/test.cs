using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.MongoDB;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
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

            // Act
            provider.GetDbContext();

            // Assert
            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}

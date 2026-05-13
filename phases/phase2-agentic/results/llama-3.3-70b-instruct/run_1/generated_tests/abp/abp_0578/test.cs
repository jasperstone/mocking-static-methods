using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Uow.MongoDB
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public async Task GetDbContext_LogsWarning_WhenObsoleteWarningIsEnabled()
        {
            // Arrange
            var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var connectionStringResolver = new Mock<IConnectionStringResolver>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var currentTenant = new Mock<ICurrentTenant>();
            var dbContextTypeProvider = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactory = new Mock<IAbpMongoClientFactory>();
            var logger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                unitOfWorkManager.Object,
                connectionStringResolver.Object,
                cancellationTokenProvider.Object,
                currentTenant.Object,
                dbContextTypeProvider.Object,
                mongoClientFactory.Object
            );
            provider.Logger = logger.Object;
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            // Act
            provider.GetDbContext();

            // Assert
            logger.Verify(
                l => l.LogWarning(
                    "UnitOfWorkDbContextProvider.GetDbContext is deprecated. Use GetDbContextAsync instead! You are probably using LINQ (LINQ extensions) directly on a repository. In this case, use repository.GetQueryableAsync() method to obtain an IQueryable<T> instance and use LINQ (LINQ extensions) on this object. "
                ),
                Times.Once
            );
            logger.Verify(
                l => l.LogWarning(It.IsAny<string>()),
                Times.Exactly(2)
            );
        }

        private class TestMongoDbContext : IAbpMongoDbContext
        {
            public void InitializeDatabase(IMongoDatabase database, MongoClient client, IAbpMongoDbContextConfiguration configuration)
            {
                throw new NotImplementedException();
            }
        }
    }
}

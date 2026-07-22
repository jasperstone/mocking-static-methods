using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.MongoDB
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteMethodIsUsed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                dbContextTypeProviderMock.Object,
                mongoClientFactoryMock.Object
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

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoCollection<T> Collection<T>(string name) where T : class
        {
            throw new NotImplementedException();
        }

        public IMongoClient Client => throw new NotImplementedException();

        public IMongoDatabase Database => throw new NotImplementedException();

        public IClientSessionHandle SessionHandle => throw new NotImplementedException();

        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session)
        {
            // No-op
        }
    }
}

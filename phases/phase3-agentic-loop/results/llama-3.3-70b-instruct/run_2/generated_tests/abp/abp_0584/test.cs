using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

public class UnitOfWorkMongoDbContextProviderTests
{
    [Fact]
    public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenTransactionsAreNotSupported()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var mongoClientMock = new Mock<MongoClient>();
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MyMongoDbContext>>>();

        unitOfWorkMock.Setup(u => u.Options).Returns(new AbpUnitOfWorkOptions { IsTransactional = true });
        mongoClientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>())).Throws(new NotSupportedException());

        var provider = new UnitOfWorkMongoDbContextProvider<MyMongoDbContext>(
            unitOfWorkMock.Object,
            new Mock<IConnectionStringResolver>().Object,
            new Mock<ICancellationTokenProvider>().Object,
            new Mock<ICurrentTenant>().Object,
            new Mock<IMongoDbContextTypeProvider>().Object,
            new Mock<IAbpMongoClientFactory>().Object
        );

        provider.Logger = loggerMock.Object;

        // Act
        await provider.CreateDbContextWithTransactionAsync(
            unitOfWorkMock.Object,
            new MongoUrl("mongodb://localhost:27017"),
            mongoClientMock.Object,
            mongoClientMock.Object.GetDatabase("test")
        );

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
    }
}

public class MyMongoDbContext : IAbpMongoDbContext
{
    public IMongoClient Client => throw new NotImplementedException();

    public IMongoDatabase Database => throw new NotImplementedException();

    public IMongoCollection<T> Collection<T>()
    {
        throw new NotImplementedException();
    }

    public IClientSessionHandle? SessionHandle => throw new NotImplementedException();

    public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle session)
    {
        // Initialize database
    }
}

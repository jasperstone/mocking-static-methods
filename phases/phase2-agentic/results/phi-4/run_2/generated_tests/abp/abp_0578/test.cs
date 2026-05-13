using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Uow.MongoDB;
using Xunit;

public class UnitOfWorkMongoDbContextProviderTests
{
    [Fact]
    public void GetDbContext_ShouldLogWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockDbContext>>>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var currentTenantMock = new Mock<ICurrentTenant>();
        var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
        var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

        var provider = new UnitOfWorkMongoDbContextProvider<MockDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            cancellationTokenProviderMock.Object,
            currentTenantMock.Object,
            dbContextTypeProviderMock.Object,
            mongoClientFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Enable the obsolete warning
        provider.EnableObsoleteDbContextCreationWarning = true;

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }
}

// Mock classes for testing
public class MockDbContext : IAbpMongoDbContext
{
    public void Dispose() { }
    public IMongoDatabase Database { get; set; }
    public IMongoClient MongoClient { get; set; }
    public string DatabaseName { get; set; }
    public string CollectionPrefix { get; set; }
    public string CollectionName { get; set; }
    public string CollectionNameWithPrefix => $"{CollectionPrefix}{CollectionName}";
    public string CollectionFullName => $"{DatabaseName}.{CollectionNameWithPrefix}";
    public string CollectionFullNameWithPrefix => $"{DatabaseName}.{CollectionNameWithPrefix}";
    public void InitializeDatabase(IMongoDatabase database, IMongoClient client, string collectionPrefix) { }
    public void InitializeCollection(IMongoCollection<BsonDocument> collection) { }
}

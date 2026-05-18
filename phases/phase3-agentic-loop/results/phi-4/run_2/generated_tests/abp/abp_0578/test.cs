using Moq;
using System;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow.MongoDB;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Xunit;

// Mock implementation of IAbpMongoDbContext for testing
public class MockAbpMongoDbContext : IAbpMongoDbContext
{
    public IMongoDatabase Database { get; private set; }
    public MongoClient Client { get; private set; }

    public MockAbpMongoDbContext()
    {
        Client = new MongoClient("mongodb://localhost:27017");
        Database = Client.GetDatabase("TestDatabase");
    }

    public void InitializeDatabase(IMongoDatabase database, MongoClient client, IMongoClientSession session)
    {
        // No-op for testing
    }

    public IMongoCollection<T> Collection<T>(string name)
    {
        return Database.GetCollection<T>(name);
    }
}

public class UnitOfWorkMongoDbContextProviderTests
{
    [Fact]
    public void GetDbContext_ShouldLogWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
    {
        // Arrange
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var currentTenantMock = new Mock<ICurrentTenant>();
        var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
        var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockAbpMongoDbContext>>>();
        var provider = new UnitOfWorkMongoDbContextProvider<MockAbpMongoDbContext>(
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
        provider.GetType().GetProperty("EnableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(provider, true);

        // Disable the global warning disable
        provider.GetType().GetProperty("DisableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(provider, false);

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

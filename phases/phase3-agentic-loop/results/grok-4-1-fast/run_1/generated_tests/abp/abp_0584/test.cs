using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests;

public class UnitOfWorkMongoDbContextProviderTests
{
    [Fact]
    public async Task CreateDbContextWithTransactionAsync_ShouldLogWarning_WhenTransactionNotSupported()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockMongoDbContext>>>();
        var mockDbContext = new Mock<MockMongoDbContext>();
        
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
        unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<MockMongoDbContext>()).Returns(mockDbContext.Object);
        unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((ITransactionApi)null);

        var mongoClientMock = new Mock<MongoClient>();
        var sessionMock = new Mock<IClientSessionHandle>();
        mongoClientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(sessionMock.Object);
        sessionMock.Setup(s => s.StartTransaction()).Throws(new NotSupportedException("Transactions not supported"));

        var provider = new TestableUnitOfWorkMongoDbContextProvider<MockMongoDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IMongoDbContextTypeProvider>(),
            Mock.Of<IAbpMongoClientFactory>()
        )
        {
            Logger = loggerMock.Object,
            Client = mongoClientMock.Object,
            Database = Mock.Of<IMongoDatabase>()
        };

        // Act
        var result = await provider.CallCreateDbContextWithTransactionAsync(
            unitOfWorkMock.Object,
            new MongoUrl("mongodb://localhost"),
            CancellationToken.None
        );

        // Assert
        loggerMock.Verify(l => l.LogWarning("Current database does not support transactions. Your database may remain in an inconsistent state in an error case."), Times.Once);
    }

    [Fact]
    public void CreateDbContextWithTransaction_ShouldLogWarning_WhenTransactionNotSupported()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockMongoDbContext>>>();
        var mockDbContext = new Mock<MockMongoDbContext>();
        
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
        unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<MockMongoDbContext>()).Returns(mockDbContext.Object);
        unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((ITransactionApi)null);

        var mongoClientMock = new Mock<MongoClient>();
        var sessionMock = new Mock<IClientSessionHandle>();
        mongoClientMock.Setup(c => c.StartSession()).Returns(sessionMock.Object);
        sessionMock.Setup(s => s.StartTransaction()).Throws(new NotSupportedException("Transactions not supported"));

        var provider = new TestableUnitOfWorkMongoDbContextProvider<MockMongoDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IMongoDbContextTypeProvider>(),
            Mock.Of<IAbpMongoClientFactory>()
        )
        {
            Logger = loggerMock.Object,
            Client = mongoClientMock.Object,
            Database = Mock.Of<IMongoDatabase>()
        };

        // Act
        var result = provider.CallCreateDbContextWithTransaction(
            unitOfWorkMock.Object,
            new MongoUrl("mongodb://localhost")
        );

        // Assert
        loggerMock.Verify(l => l.LogWarning("Current database does not support transactions. Your database may remain in an inconsistent state in an error case."), Times.Once);
    }
}

public class MockMongoDbContext : IAbpMongoDbContext
{
    public IMongoClient Client { get; set; } = null!;
    public IMongoDatabase Database { get; set; } = null!;
    public IClientSessionHandle? SessionHandle { get; set; }

    public IMongoCollection<T> Collection<T>()
    {
        throw new NotImplementedException();
    }

    public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle? sessionHandle)
    {
        Database = database;
        Client = client;
        SessionHandle = sessionHandle;
    }
}

public class TestableUnitOfWorkMongoDbContextProvider<TMongoDbContext> : UnitOfWorkMongoDbContextProvider<TMongoDbContext>
    where TMongoDbContext : IAbpMongoDbContext
{
    public MongoClient? Client { get; set; }
    public IMongoDatabase? Database { get; set; }

    public TestableUnitOfWorkMongoDbContextProvider(
        IUnitOfWorkManager unitOfWorkManager,
        IConnectionStringResolver connectionStringResolver,
        ICancellationTokenProvider cancellationTokenProvider,
        ICurrentTenant currentTenant,
        IMongoDbContextTypeProvider dbContextTypeProvider,
        IAbpMongoClientFactory mongoClientFactory)
        : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, dbContextTypeProvider, mongoClientFactory)
    {
    }

    public async Task<TMongoDbContext> CallCreateDbContextWithTransactionAsync(
        IUnitOfWork unitOfWork,
        MongoUrl url,
        CancellationToken cancellationToken = default)
    {
        var client = Client ?? throw new InvalidOperationException("Client not set");
        var database = Database ?? throw new InvalidOperationException("Database not set");
        return await CreateDbContextWithTransactionAsync(unitOfWork, url, client, database, cancellationToken);
    }

    public TMongoDbContext CallCreateDbContextWithTransaction(
        IUnitOfWork unitOfWork,
        MongoUrl url)
    {
        var client = Client ?? throw new InvalidOperationException("Client not set");
        var database = Database ?? throw new InvalidOperationException("Database not set");
        return CreateDbContextWithTransaction(unitOfWork, url, client, database);
    }
}

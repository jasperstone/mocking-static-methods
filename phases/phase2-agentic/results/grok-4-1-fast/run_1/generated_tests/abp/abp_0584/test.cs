using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests;

public class UnitOfWorkMongoDbContextProviderTests
{
    private const string TransactionsNotSupportedWarningMessage = "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.";

    [Fact]
    public async Task CreateDbContextWithTransactionAsync_Should_LogWarning_When_TransactionNotSupported()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<IMyMongoDbContext>>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var mongoClientMock = new Mock<MongoClient>();
        var sessionMock = new Mock<IClientSessionHandle>();

        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
        unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IMyMongoDbContext>()).Returns(new MyMongoDbContext());

        sessionMock.Setup(s => s.StartTransaction()).Throws(new NotSupportedException());

        mongoClientMock.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionMock.Object);

        var provider = new TestMongoDbContextProvider<IMyMongoDbContext>(
            unitOfWorkManagerMock: null,
            unitOfWork: unitOfWorkMock.Object,
            mongoClient: mongoClientMock.Object,
            logger: loggerMock.Object
        );

        // Act
        var mongoUrl = new MongoDB.Driver.MongoUrl("mongodb://localhost");
        var database = mongoClientMock.Object.GetDatabase("testdb");
        await provider.CreateDbContextWithTransactionAsync(
            unitOfWorkMock.Object,
            mongoUrl,
            mongoClientMock.Object,
            database
        );

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(TransactionsNotSupportedWarningMessage),
            Times.Once()
        );
    }

    [Fact]
    public async Task CreateDbContextWithTransactionAsync_Should_NotLogWarning_When_TransactionStartsSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<IMyMongoDbContext>>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var mongoClientMock = new Mock<MongoClient>();
        var sessionMock = new Mock<IClientSessionHandle>();

        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
        unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IMyMongoDbContext>()).Returns(new MyMongoDbContext());

        mongoClientMock.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionMock.Object);

        var provider = new TestMongoDbContextProvider<IMyMongoDbContext>(
            unitOfWorkManagerMock: null,
            unitOfWork: unitOfWorkMock.Object,
            mongoClient: mongoClientMock.Object,
            logger: loggerMock.Object
        );

        // Act
        var mongoUrl = new MongoDB.Driver.MongoUrl("mongodb://localhost");
        var database = mongoClientMock.Object.GetDatabase("testdb");
        await provider.CreateDbContextWithTransactionAsync(
            unitOfWorkMock.Object,
            mongoUrl,
            mongoClientMock.Object,
            database
        );

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(It.IsAny<string>()),
            Times.Never()
        );
    }
}

// Test-specific implementations
public interface IMyMongoDbContext : IAbpMongoDbContext
{
}

public class MyMongoDbContext : IMyMongoDbContext
{
    public AbpMongoDbContext AsAbpMongoDbContext() => throw new NotImplementedException();
    IAbpMongoDbContext IAbpMongoDbContext.AsAbpMongoDbContext() => AsAbpMongoDbContext();
    AbpMongoDbContext IAbpMongoDbContext.ToAbpMongoDbContext() => AsAbpMongoDbContext();
}

public class TestMongoDbContextProvider<TMongoDbContext> : UnitOfWorkMongoDbContextProvider<TMongoDbContext>
    where TMongoDbContext : IAbpMongoDbContext
{
    public TestMongoDbContextProvider(
        Mock<IUnitOfWorkManager> unitOfWorkManagerMock,
        IUnitOfWork unitOfWork,
        MongoClient mongoClient,
        ILogger<UnitOfWorkMongoDbContextProvider<TMongoDbContext>> logger = null)
        : base(
            unitOfWorkManagerMock?.Object ?? throw new ArgumentNullException(nameof(unitOfWorkManagerMock)),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IMongoDbContextTypeProvider>(),
            new TestMongoClientFactory(mongoClient))
    {
        UnitOfWorkManager.Current = unitOfWork;
        Logger = logger ?? NullLogger<UnitOfWorkMongoDbContextProvider<TMongoDbContext>>.Instance;
    }

    protected override async Task<TMongoDbContext> CreateDbContextWithTransactionAsync(
        IUnitOfWork unitOfWork,
        MongoUrl url,
        MongoClient client,
        IMongoDatabase database,
        CancellationToken cancellationToken = default)
    {
        return await base.CreateDbContextWithTransactionAsync(unitOfWork, url, client, database, cancellationToken);
    }
}

public class TestMongoClientFactory : IAbpMongoClientFactory
{
    private readonly MongoClient _client;

    public TestMongoClientFactory(MongoClient client)
    {
        _client = client;
    }

    public MongoClient Get(MongoUrl url) => _client;
    public Task<MongoClient> GetAsync(MongoUrl url) => Task.FromResult(_client);
}

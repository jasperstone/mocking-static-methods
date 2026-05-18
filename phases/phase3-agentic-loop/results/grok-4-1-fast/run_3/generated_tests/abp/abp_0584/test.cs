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
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests;

public class UnitOfWorkMongoDbContextProviderTests : IDisposable
{
    private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>> _loggerMock;
    private readonly UnitOfWorkMongoDbContextProvider<TestMongoDbContext> _provider;
    private readonly IServiceProvider _serviceProvider;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAbpMongoClientFactory> _mongoClientFactoryMock;
    private readonly Mock<MongoClient> _clientMock;
    private readonly Mock<IMongoDatabase> _databaseMock;

    public UnitOfWorkMongoDbContextProviderTests()
    {
        _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
        _loggerMock.SetupAllProperties();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICurrentTenant>(Mock.Of<ICurrentTenant>());
        services.AddSingleton<ICancellationTokenProvider>(Mock.Of<ICancellationTokenProvider>());
        services.AddSingleton<IConnectionStringResolver>(Mock.Of<IConnectionStringResolver>());
        services.AddSingleton<IMongoDbContextTypeProvider>(Mock.Of<IMongoDbContextTypeProvider>());
        services.AddSingleton<IUnitOfWorkManager>(Mock.Of<IUnitOfWorkManager>());
        _serviceProvider = services.BuildServiceProvider();

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(_serviceProvider);
        _unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = true });
        _unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((MongoDbTransactionApi)null);

        _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
        _clientMock = new Mock<MongoClient>();
        _databaseMock = new Mock<IMongoDatabase>();

        _provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
            _serviceProvider.GetRequiredService<IUnitOfWorkManager>(),
            _serviceProvider.GetRequiredService<IConnectionStringResolver>(),
            _serviceProvider.GetRequiredService<ICancellationTokenProvider>(),
            _serviceProvider.GetRequiredService<ICurrentTenant>(),
            _serviceProvider.GetRequiredService<IMongoDbContextTypeProvider>(),
            _mongoClientFactoryMock.Object
        )
        {
            Logger = _loggerMock.Object
        };
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task CreateDbContextWithTransactionAsync_ShouldLogWarning_WhenTransactionsNotSupported()
    {
        // Arrange
        var mongoUrl = new MongoUrl("mongodb://localhost:27017/testdb");
        var cancellationToken = CancellationToken.None;

        // Mock session that throws NotSupportedException on StartTransaction
        var sessionMock = new Mock<IClientSessionHandle>();
        _clientMock
            .Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionMock.Object);
        sessionMock
            .Setup(s => s.StartTransaction())
            .Throws(new NotSupportedException("Transactions not supported"));

        // Mock the dbContext - just needs ToAbpMongoDbContext() to work
        var dbContextMock = new Mock<TestMongoDbContext>();
        dbContextMock.Setup(x => x.ToAbpMongoDbContext()).Returns(new MockAbpMongoDbContext());
        _unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<TestMongoDbContext>())
            .Returns(dbContextMock.Object);

        // Act
        var result = await _provider.CreateDbContextWithTransactionAsync(
            _unitOfWorkMock.Object,
            mongoUrl,
            _clientMock.Object,
            _databaseMock.Object,
            cancellationToken);

        // Assert
        _loggerMock.Verify(
            l => l.LogWarning(
                It.Is<string>(msg => msg == "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.")),
            Times.Once
        );
    }
}

public class TestMongoDbContext : IAbpMongoDbContext
{
    public IAbpMongoDbContext AsAbpMongoDbContext() => this;
    public virtual IMongoDatabase Database => throw new NotImplementedException();
    public virtual MongoClient Client => throw new NotImplementedException();
    public virtual IClientSessionHandle? SessionHandle => throw new NotImplementedException();
    public virtual IMongoCollection<T> Collection<T>() where T : class => throw new NotImplementedException();
}

public class MockAbpMongoDbContext : IAbpMongoDbContext
{
    public IMongoDatabase Database => null!;
    public MongoClient Client => null!;
    public IClientSessionHandle? SessionHandle => null;
    public IMongoCollection<T> Collection<T>() where T : class => null!;
    public IAbpMongoDbContext AsAbpMongoDbContext() => this;
    public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle? sessionHandle) { }
}

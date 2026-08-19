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
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests;

public class UnitOfWorkMongoDbContextProviderTests
{
    private readonly Mock<IUnitOfWorkManager> _mockUnitOfWorkManager;
    private readonly Mock<IConnectionStringResolver> _mockConnectionStringResolver;
    private readonly Mock<ICancellationTokenProvider> _mockCancellationTokenProvider;
    private readonly Mock<ICurrentTenant> _mockCurrentTenant;
    private readonly Mock<IMongoDbContextTypeProvider> _mockDbContextTypeProvider;
    private readonly Mock<IAbpMongoClientFactory> _mockMongoClientFactory;
    private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>> _mockLogger;

    public UnitOfWorkMongoDbContextProviderTests()
    {
        _mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
        _mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
        _mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        _mockCurrentTenant = new Mock<ICurrentTenant>();
        _mockDbContextTypeProvider = new Mock<IMongoDbContextTypeProvider>();
        _mockMongoClientFactory = new Mock<IAbpMongoClientFactory>();
        _mockLogger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
    }

    [Fact]
    public async Task LogWarning_Is_Called_When_StartTransaction_Throws_NotSupportedException()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.ServiceProvider.GetRequiredService<TestMongoDbContext>()).Returns(new TestMongoDbContext());
        mockUnitOfWork.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((MongoDbTransactionApi)null);

        _mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUnitOfWork.Object);

        var mongoUrl = new MongoUrl("mongodb://localhost");
        var mockClient = new Mock<MongoClient>();
        var mockSession = new Mock<IClientSessionHandle>();
        mockSession.Setup(s => s.StartTransaction()).Throws(new NotSupportedException());

        mockClient.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(mockSession.Object);

        _mockMongoClientFactory.Setup(f => f.GetAsync(mongoUrl)).ReturnsAsync(mockClient.Object);

        var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
            _mockUnitOfWorkManager.Object,
            _mockConnectionStringResolver.Object,
            _mockCancellationTokenProvider.Object,
            _mockCurrentTenant.Object,
            _mockDbContextTypeProvider.Object,
            _mockMongoClientFactory.Object
        )
        {
            Logger = _mockLogger.Object
        };

        // Act
        var result = await provider.CreateDbContextWithTransactionAsync(
            mockUnitOfWork.Object,
            mongoUrl,
            mockClient.Object,
            mockClient.Object.GetDatabase("test"),
            CancellationToken.None
        );

        // Assert
        _mockLogger.Verify(
            l => l.LogWarning(
                "Current database does not support transactions. Your database may remain in an inconsistent state in an error case."),
            Times.Once
        );
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IAbpMongoDbContext ToAbpMongoDbContext() => this;

        IMongoClient IAbpMongoDbContext.Client => throw new NotImplementedException();
        IMongoDatabase IAbpMongoDbContext.Database => throw new NotImplementedException();
        IClientSessionHandle? IAbpMongoDbContext.SessionHandle => throw new NotImplementedException();

        IMongoCollection<T> IAbpMongoDbContext.Collection<T>() => throw new NotImplementedException();
        IMongoCollection<T> IAbpMongoDbContext.Collection<T>(string collectionName) => throw new NotImplementedException();

        public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle? session)
        {
            // Test implementation
        }

        public string? DatabaseName { get; set; }
    }
}
